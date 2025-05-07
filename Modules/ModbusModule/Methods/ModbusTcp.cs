using ModbusModule.Methods.Interface;
using ModbusModule.Models;
using ModbusModule.ViewModels;
using NModbus;
using NModbus.Device;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Windows.Markup;

namespace ModbusModule.Methods
{
    /// <summary>
    /// 负责管理 Modbus TCP 连接、状态和事件。
    /// </summary>
    public class ModbusTcp : IModbusTcp, IDisposable
    {

        #region ****************************** 配置信息 ******************************

        // 使用 readonly lock 对象来保护共享资源
        private readonly object _lock = new object();

        private TcpClient? _tcpClient;
        private IModbusMaster? _master;
        private Task? _communicationTask;
        private CancellationTokenSource? _cts;

        // _isConnected 状态的改变需要锁保护
        private bool _isConnected;

        public bool IsConnected
        {
            get
            {
                lock (_lock) // 读取也需要锁保护，确保获取最新值
                {
                    return _isConnected;
                }
            }
            private set
            {
                // 设置时加锁
                lock (_lock)
                {
                    if (_isConnected != value)
                    {
                        _isConnected = value;
                        // 事件触发通常不需要在锁内部，但在这里为了确保状态改变和事件同步，放在锁内部也可以。
                        // 更高级的模式是使用 ConcurrentQueue 或 Producer/Consumer 模式将事件触发放到另一个线程，
                        // 以免事件处理函数耗时过长阻塞当前锁。但对于简单的事件，放在锁内部是可以接受的。
                        Task.Run(() => ConnectionStatusChanged?.Invoke(this, _isConnected)); // 使用Task.Run异步触发事件，避免阻塞
                    }
                }
            }
        }

        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler<Exception>? ErrorOccurred;

        // Requester 属性需要锁保护其 get 访问器，因为 _requester 可能会被置为 null
        private IModbusRequester? _requester;
        public IModbusRequester Requester
        {
            get
            {
                lock (_lock)
                {
                    if (_requester == null)
                    {
                        throw new InvalidOperationException("Modbus TCP is not connected or requester is not initialized.");
                    }
                    return _requester;
                }
            }
            private set // 只有内部可以设置
            {
                lock (_lock)
                {
                    _requester = value;
                }
            }
        }

        public ModbusConfig? ModbusConfig { get; set; }

        #endregion ****************************** 配置信息 ******************************

        #region ****************************** 连接创建和轮询逻辑 ******************************

        public async Task ConnectAsync(ModbusConfig configs, ModbusTimes options, LogMessage LogMessage, 
            ObservableCollection<ModbusItems> InputItems, ObservableCollection<ModbusItems> OutputItems,
            SynchronizationContext uiContext)
        {
            // 在尝试连接前加锁，避免多重连接尝试
            lock (_lock)
            {
                if (_isConnected) // 检查 _isConnected 状态
                {
                    // 如果已经连接，但可能后台任务已经停止，清理并尝试重新连接
                    // 或者更简单的，如果 IsConnected 为 true 就直接返回
                    // 根据需求选择策略，这里选择如果 IsConnected 为 true 则直接返回
                    return;
                }

                // 如果连接状态为 false，但资源可能没完全清理干净（异常退出等），先清理一下
                // 避免重复创建 TcpClient 等
                CleanupResources();

                _cts = new CancellationTokenSource();
            }

            var cancellationToken = _cts.Token;

            try
            {
                TcpClient? client = null;
                IModbusMaster? master = null;
                IModbusRequester? requester = null;
                Task? communicationTask = null;
                ModbusConfig = configs; // 设置 ModbusConfig

                client = new TcpClient();
                // ConnectAsync 是异步的，不阻塞调用线程
                await client.ConnectAsync(configs.IPAddress, configs.Port, cancellationToken).ConfigureAwait(false);

                // 连接成功后，设置超时
                client.ReceiveTimeout = options.ReadTimeoutMs;
                client.SendTimeout = options.WriteTimeoutMs;

                var factory = new ModbusFactory();
                master = factory.CreateMaster(client);

                // 创建 Requester。确保访问 _master 是线程安全的，因为 Disconnect 可能会将其置为 null
                requester = new ModbusRequester(() =>
                {
                    lock (_lock) // 在访问 _master 前加锁
                    {
                        if (!_isConnected || _master == null)
                        {
                            throw new InvalidOperationException("Modbus TCP is not connected.");
                        }
                        return _master;
                    }
                });

                var portRanges = PollingMethod.GetPortRanges(InputItems, OutputItems);

                // 在设置所有成员变量前加锁，确保原子性
                lock (_lock)
                {
                    if (_cts == null || _cts.IsCancellationRequested)
                    {
                        // 如果在等待连接或创建 master/requester 时，对象被 Dispose 或 Disconnect 了
                        client.Close(); // 清理已创建的资源
                        master?.Dispose();
                        throw new OperationCanceledException("Connection attempt canceled.");
                    }

                    _tcpClient = client;
                    _master = master;
                    Requester = requester; // 使用属性的 setter
                    IsConnected = true; // 使用属性的 setter

                    // 启动后台通信任务
                    // Task.Run 启动一个独立的任务，不会阻塞 ConnectAsync 的调用者
                    _communicationTask = Task.Run(() => CommunicationLoop( 
                        configs,
                        options, 
                        LogMessage, 
                        portRanges,
                        InputItems,
                        OutputItems,
                        uiContext,
                        cancellationToken
                        ), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 连接或等待过程中被取消，正常退出
                lock (_lock)
                {
                    IsConnected = false; // 使用属性的 setter
                    CleanupResources(); // 清理可能部分创建的资源
                }
                throw; // 重新抛出取消异常
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    IsConnected = false; // 使用属性的 setter
                    ErrorOccurred?.Invoke(this, ex);
                    // 确保清理资源，即使在连接失败时任务没有启动
                    CleanupResources();
                }
                throw; // 重新抛出异常
            }
        }

        // CommunicationLoop 方法现在接收 ConnectAsync 传递过来的参数
        private async Task CommunicationLoop(
            ModbusConfig configs,
            ModbusTimes options,
            LogMessage  LogMessage,
            Dictionary<string, (ushort MinAddress, ushort MaxAddress)> portRanges,
            ObservableCollection<ModbusItems> inputItems,
            ObservableCollection<ModbusItems> outputItems,
            SynchronizationContext uiContext,
            CancellationToken cancellationToken)
        {
            LogMessage.AppendLogMessage("Modbus 通信启动成功");

            // 可以创建一个字典或其他结构来存储上次成功读取的数据，用于比较变化
            // 示例: Dictionary<string, Dictionary<ushort, object>> lastKnownData = new Dictionary<string, Dictionary<ushort, object>>();
            // string key 可以是 "InputCoil", "HoldingRegister" 等

            Dictionary<string, Array> lastKnownData = new Dictionary<string, Array>();

            try
            {
                // 在 CommunicationLoop 任务执行期间，IsConnected 应该为 true，直到外部调用 Disconnect 或 Dispose
                // 循环条件只依赖于 cancellationToken
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 在每次轮询开始前，确保连接仍然被认为是活动的
                    // 如果 IsConnected 变为 false，说明外部发起了断开或发生了致命错误
                    // 虽然循环条件依赖 cancellationToken，但在这里检查 IsConnected 可以更早发现断开状态
                    if (!IsConnected) // 使用属性的 getter，内部有锁
                    {
                        LogMessage.AppendLogMessage("Modbus 连接断开");
                        break; // 退出循环
                    }


                    try
                    {
                        // 获取 Requester 实例，属性 getter 内部有锁和 null 检查
                        var currentRequester = Requester;

                        // 遍历需要轮询的地址范围字典
                        foreach (var range in portRanges)
                        {
                            string dataTypeKey = range.Key;
                            ushort startAddress = range.Value.MinAddress;
                            ushort endAddress = range.Value.MaxAddress;
                            int numberOfPoints = endAddress - startAddress + 1;

                            Array? modbusData = null; // 用于存储读取的原始 Modbus 数据

                            try
                            {
                                // 根据数据类型执行相应的读取操作
                                // configs.SlaveID 用于指定从站地址
                                switch (dataTypeKey)
                                {
                                    case "DiscreteInput": // Discrete Inputs (功能码 2) - NModbus 叫 ReadInputs
                                        modbusData = await currentRequester.ReadInputsAsync(configs.SlaveID, startAddress, (ushort)numberOfPoints).ConfigureAwait(false);
                                        break;
                                    case "InputRegister": // Input Registers (功能码 4)
                                        modbusData = await currentRequester.ReadInputRegistersAsync(configs.SlaveID, startAddress, (ushort)numberOfPoints).ConfigureAwait(false);
                                        break;
                                    case "Coil": // Coils (功能码 1) - NModbus 叫 ReadCoils
                                        modbusData = await currentRequester.ReadCoilsAsync(configs.SlaveID, startAddress, (ushort)numberOfPoints).ConfigureAwait(false);
                                        break;
                                    case "HoldingRegister": // Holding Registers (功能码 3)
                                        modbusData = await currentRequester.ReadHoldingRegistersAsync(configs.SlaveID, startAddress, (ushort)numberOfPoints).ConfigureAwait(false);
                                        break;
                                    default:
                                        continue; // 跳过未知类型
                                }

                                // TODO: 比较新读取的数据 (modbusData) 和上次成功读取的数据 (lastKnownData)
                                bool dataChanged = true; // 假设数据已改变，需要实现比较逻辑

                                if (modbusData != null) // 确保成功读取到数据
                                {
                                    if (lastKnownData.TryGetValue(dataTypeKey, out var previousData))
                                    {
                                        // 如果之前读取过同类型的数据
                                        // 检查数组长度是否一致 (理论上同一个范围长度不会变)
                                        if (previousData != null && previousData.Length == modbusData.Length)
                                        {
                                            // 比较数组内容是否完全一致
                                            // 使用 Array.IStructuralComparable.Equals 方法可以比较不同维度的数组，
                                            // 但对于一维数组，转换后再用 SequenceEqual 更直观且类型安全。
                                            // 由于你要求不使用枚举等，我们直接根据可能是 bool[] 或 ushort[] 来比较。
                                            if (modbusData is bool[] currentBoolData && previousData is bool[] previousBoolData)
                                            {
                                                dataChanged = !currentBoolData.SequenceEqual(previousBoolData);
                                            }
                                            else if (modbusData is ushort[] currentUshortData && previousData is ushort[] previousUshortData)
                                            {
                                                dataChanged = !currentUshortData.SequenceEqual(previousUshortData);
                                            }
                                            // else { dataChanged remains true; // 数据类型不匹配，认为是变化 }
                                        }
                                        // else { dataChanged remains true; // 长度不一致或 previousData 为 null，认为是变化 }
                                    }
                                    // else { dataChanged remains true; // 第一次读取，认为是变化 }
                                }
                                else // 如果 modbusData 为 null，说明读取失败，不算数据变化
                                {
                                    dataChanged = false; // 读取失败不认为是数据变化
                                }


                                /// 如果数据发生改变，更新 lastKnownData 并调用静态方法更新 InputItems / OutputItems
                                if (dataChanged)
                                {
                                    LogMessage.AppendLogMessage($"{dataTypeKey.PadRight(16)}：" + (modbusData is Array array ? string.Join(", ", array.Cast<object>()) : string.Empty));
                                    // 更新上次已知数据
                                    lastKnownData[dataTypeKey] = modbusData!; // modbusData 在这里不会为 null，因为 dataChanged == true 隐含 modbusData != null

                                    // 调用静态类的静态方法处理数据转换和更新
                                    ObservableCollection<ModbusItems>? targetCollection = null;



                                    // 根据 dataTypeKey 确定更新哪个集合 (inputItems 或 outputItems)
                                    // 注意这里根据 GetPortRanges 中的 key 来判断
                                    if (dataTypeKey == "DiscreteInput" || dataTypeKey == "InputRegister" || dataTypeKey == "WriteMultipleCoils" || dataTypeKey == "WriteMultipleRegisters")
                                    {
                                        targetCollection = inputItems;
                                    }
                                    else if (dataTypeKey == "Coil" || dataTypeKey == "HoldingRegister")
                                    {
                                        targetCollection = outputItems;
                                    }

                                    if (targetCollection != null)
                                    {
                                        // TODO: 调用你的 ModbusDataProcessor 静态方法来更新 targetCollection 中的 ModbusItems
                                        // LogMessage.AppendLogMessage($"Data changed, calling processor for {dataTypeKey} at address {startAddress}...", LogLevel.Debug); // 使用你的日志方法
                                        // ModbusDataProcessor.UpdateItemsFromModbusData(targetCollection, configs.SlaveID, dataTypeEnum, startAddress, modbusData); // 调用你的处理方法，传递 dataTypeEnum 如果需要


                                        // 重要：如果 targetCollection 绑定到 UI 控件，这里的更新必须在 UI 线程上进行！
                                        // 确保 ModbusDataProcessor.UpdateItemsFromModbusData 方法内部正确处理了 UI 线程封送。
                                        // *** 使用 UI 线程上下文将更新操作调度回 UI 线程 ***
                                        uiContext.Post(state =>
                                        {
                                            // 这个 Lambda 表达式将在 UI 线程上执行
                                            // state 参数是 Post 传递的第二个参数，这里用不到可以传 null

                                            // 在这里调用你的 ModbusDataProcessor.UpdateItemsFromModbusData 方法
                                            // 或者直接在这里修改 targetCollection 及其项的属性
                                            PollingMethod.UpdateItemsFromModbusData(targetCollection, dataTypeKey, startAddress, modbusData);

                                            // 示例：更新集合中的某个项
                                            // var itemToUpdate = targetCollection.FirstOrDefault(...);
                                            // if (itemToUpdate != null)
                                            // {
                                            //     itemToUpdate.Value = newValue; // 假设 ModbusItem 实现了 INotifyPropertyChanged
                                            // }

                                        }, null); // null 作为 state 参数
                                    }

                                }
                                else
                                {
                                    // 只有在成功读取且数据未变化时记录此信息，避免在读取失败时也记录"未变化"
                                    if (modbusData != null)
                                    {
                                        // LogMessage.AppendLogMessage($"Data unchanged for {dataTypeKey} at address {startAddress} to {endAddress}.", LogLevel.Debug); // 使用你的日志方法
                                    }
                                }
                            }
                            catch (Exception modbusReadEx)
                            {
                                // 处理 Modbus 读取过程中的异常（例如：超时，从站无响应，协议错误等）
                                // 这类异常通常不应导致整个连接断开，而是记录错误并继续轮询下一个点或下一个轮询周期
                                LogMessage.AppendLogMessage($"Modbus 读取错误，请断开重新连接: {modbusReadEx.Message}");
                                await Task.Delay(options.WriteTimeoutMs, cancellationToken).ConfigureAwait(false);
                                // 触发 ErrorOccurred 事件 (可选，取决于你想如何处理单点/单范围读取错误)
                                // ErrorOccurred?.Invoke(this, new Exception($"Modbus read error for {dataTypeKey} at address {startAddress}.", modbusReadEx));
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Requester 抛出此异常表示连接已断开 (_master 为 null)，循环应该退出
                        LogMessage.AppendLogMessage("连接丢失，正在退出连接");
                        break;
                    }
                    catch (SocketException sockEx)
                    {
                        // 处理 Socket 级别的异常，可能意味着连接已断开
                        LogMessage.AppendLogMessage($"服务端异常，连接已断开: {sockEx.Message}");
                        lock (_lock)
                        {
                            IsConnected = false; // 使用属性的 setter
                        }
                        // 触发 ErrorOccurred 事件
                        ErrorOccurred?.Invoke(this, new Exception("Socket error during communication loop.", sockEx));
                        break; // 退出循环
                    }
                    catch (Exception ex)
                    {
                        // 捕获通信循环中其他未预料的异常
                        LogMessage.AppendLogMessage($"未预料的异常: {ex.Message}");
                        lock (_lock)
                        {
                            IsConnected = false; // 使用属性性 setter
                        }
                        // 触发 ErrorOccurred 事件
                        ErrorOccurred?.Invoke(this, new Exception("Unexpected error in communication loop.", ex));
                        break; // 退出循环
                    }

                    // 轮询间隔延迟
                    // 使用 PollIntervalMs 作为延迟时间
                    await Task.Delay(options.PollIntervalMs, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                // 任务被取消，正常退出循环
                LogMessage.AppendLogMessage("任务被取消，正常退出循环");
            }
            catch (Exception ex)
            {
                // 捕获 CommunicationLoop 外部的异常（极少发生，可能与 Task 自身状态有关）
                LogMessage.AppendLogMessage($"通信外部发生异常: {ex.Message}");
                lock (_lock)
                {
                    IsConnected = false; // Use the property setter
                }
                ErrorOccurred?.Invoke(this, new Exception("Communication loop terminated due to an unexpected error.", ex));
            }
            finally
            {
                // CommunicationLoop 任务结束（正常取消或异常退出）后，
                // 最终的资源清理由 Dispose 方法负责，而不是在这里直接清理。
                // 在这里可以做一些任务结束的收尾工作（如果需要）。
                LogMessage.AppendLogMessage("通信正常退出");
            }
        }


        #endregion ****************************** 连接创建和轮询逻辑 ******************************

        #region ****************************** 连接断开逻辑 ******************************

        // Disconnect 方法应该是非阻塞的
        public void Disconnect()
        {
            lock (_lock) // 加锁保护状态改变和 cts 的访问
            {
                if (!_isConnected)
                {
                    return;
                }

                // 标记为断开中/已断开
                IsConnected = false; // 使用属性的 setter

                // 发送取消信号给后台任务
                _cts?.Cancel();

                // 注意：这里不等待后台任务结束。让 Dispose 方法处理等待和清理。
                System.Diagnostics.Debug.WriteLine("Disconnect called, cancellation signaled.");
            }
        }

        // 清理资源的方法，由 Dispose 调用
        private void CleanupResources()
        {
            lock (_lock) // 清理资源时需要锁保护
            {
                // 确保在清理前任务已经停止或正在停止
                // 在 Dispose 中会等待任务，但在连接失败的 catch 块中直接调用时任务可能未启动或已失败

                _master?.Dispose();
                _master = null;

                _tcpClient?.Close(); // TcpClient 的 Close 方法也会释放底层资源
                _tcpClient = null;

                _requester = null; // 直接设置成员变量，因为属性 setter 内部有锁

                _cts?.Dispose();
                _cts = null;

                _communicationTask = null; // 清理任务引用
            }
            System.Diagnostics.Debug.WriteLine("Resources cleaned up.");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 在 Dispose 中加锁，确保 Dispose 过程是线程安全的
                lock (_lock)
                {
                    // 1. 标记为已断开状态（如果还没有）
                    IsConnected = false; // 使用属性 setter

                    // 2. 发送取消信号给后台任务
                    _cts?.Cancel();

                    // 3. 等待后台通信任务完成
                    // Wait() 是阻塞的，但在 Dispose 中是可以接受的，因为它通常在对象生命周期结束时调用
                    try
                    {
                        // 设置一个合理的等待超时时间
                        _communicationTask?.Wait(TimeSpan.FromSeconds(10)); // 10 秒超时
                    }
                    catch (TaskCanceledException)
                    {
                        // 任务因取消信号而结束，预期行为
                    }
                    catch (AggregateException ae)
                    {
                        // 处理等待任务时可能发生的其他异常
                        foreach (var ex in ae.Flatten().InnerExceptions)
                        {
                            ErrorOccurred?.Invoke(this, new Exception("Error while waiting for communication task to stop during Dispose.", ex));
                        }
                    }
                    catch (Exception ex)
                    {
                        // 处理等待任务时可能发生的其他异常
                        ErrorOccurred?.Invoke(this, new Exception("Error while waiting for communication task to stop during Dispose.", ex));
                    }

                    // 4. 清理资源
                    CleanupResources();

                    System.Diagnostics.Debug.WriteLine("Dispose finished.");
                }
            }
            // 如果是 finalizer 调用（disposing == false），通常不需要清理托管资源，
            // 但 TcpClient 和 ModbusMaster 需要清理非托管资源，
            // 不过标准的 Dispose 模式中，CleanupResources 会被调用（通过 Dispose(true)），
            // 如果没有调用 Dispose，finalizer 调用 Dispose(false) 时不会进入上面的 if 块。
            // 对于包含非托管资源的类，Finalize 方法应该调用 Dispose(false)，但这需要实现 Finalize 方法。
            // 对于本例，依赖 Dispose() 被正确调用进行清理即可。
        }

        public void Dispose()
        {
            // 调用 Dispose(true) 执行清理
            Dispose(true);
            // 阻止 Finalizer 的调用，因为资源已经被清理
            GC.SuppressFinalize(this);
        }


        #endregion ****************************** 连接断开逻辑 ******************************

    }
}