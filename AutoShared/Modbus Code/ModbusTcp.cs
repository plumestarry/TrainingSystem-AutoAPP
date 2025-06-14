using ModbusModule.Methods.Interface;
using ModbusModule.Models;
using ModbusModule.ViewModels;
using NModbus;
using NModbus.Device;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Windows.Markup;
namespace ModbusModule.Methods;
public class ModbusTcp : IModbusTcp, IDisposable
{
    #region ***** 配置信息 *****
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
            lock (_lock)
            {
                return _isConnected;
            }
        }
        private set
        {
            lock (_lock)
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    Task.Run(() => 
                        ConnectionStatusChanged?.Invoke(this, _isConnected));
                }
            }
        }
    }
    public event EventHandler<bool>? ConnectionStatusChanged;
    public event EventHandler<Exception>? ErrorOccurred;
    // Requester 属性需要锁保护其 get 访问器
    private IModbusRequester? _requester;
    public IModbusRequester Requester
    {
        get
        {
            lock (_lock)
            {
                if (_requester == null)
                {
                    throw new InvalidOperationException("");
                }
                return _requester;
            }
        }
        private set
        {
            lock (_lock)
            {
                _requester = value;
            }
        }
    }
    public ModbusConfig? ModbusConfig { get; set; }
    #endregion ***** 配置信息 *****

    #region ***** 连接创建和轮询逻辑 *****
    public async Task ConnectAsync(ModbusConfig configs, ModbusTimes options, 
        LogMessage LogMessage, 
        ObservableCollection<ModbusItems> InputItems, 
        ObservableCollection<ModbusItems> OutputItems,
        SynchronizationContext uiContext)
    {
        // 在尝试连接前加锁，避免多重连接尝试
        lock (_lock)
        {
            if (_isConnected)
            {
                return;
            }
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
            ModbusConfig = configs;
            client = new TcpClient();
            await client.ConnectAsync(configs.IPAddress, configs.Port, 
                cancellationToken).ConfigureAwait(false);
            // 连接成功后，设置超时
            client.ReceiveTimeout = options.ReadTimeoutMs;
            client.SendTimeout = options.WriteTimeoutMs;
            var factory = new ModbusFactory();
            master = factory.CreateMaster(client);
            requester = new ModbusRequester(() =>
            {
                lock (_lock)
                {
                    if (!_isConnected || _master == null)
                    {
                        throw new InvalidOperationException("");
                    }
                    return _master;
                }
            });
            var portRanges = PollingMethod.GetPortRanges
                (InputItems, OutputItems);
            // 在设置所有成员变量前加锁，确保原子性
            lock (_lock)
            {
                if (_cts == null || _cts.IsCancellationRequested)
                {
                    client.Close();
                    master?.Dispose();
                    throw new OperationCanceledException("");
                }
                _tcpClient = client;
                _master = master;
                Requester = requester;
                IsConnected = true;
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
            lock (_lock)
            {
                IsConnected = false;
                CleanupResources();
            }
            throw;
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                IsConnected = false;
                ErrorOccurred?.Invoke(this, ex);
                CleanupResources();
            }
            throw;
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
        Dictionary<string, Array> lastKnownData = new Dictionary<string, Array>();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!IsConnected)
                {
                    LogMessage.AppendLogMessage("Modbus 连接断开");
                    break;
                }
                try
                {
                    // 获取 Requester 实例
                    var currentRequester = Requester;
                    // 遍历需要轮询的地址范围字典
                    foreach (var range in portRanges)
                    {
                        string dataTypeKey = range.Key;
                        ushort startAddress = range.Value.MinAddress;
                        ushort endAddress = range.Value.MaxAddress;
                        int numberOfPoints = endAddress - startAddress + 1;
                        Array? modbusData = null;
                        try
                        {
                            // 根据数据类型执行相应的读取操作
                            switch (dataTypeKey)
                            {
                                case "DiscreteInput":
                                    modbusData = await currentRequester
                                        .ReadInputsAsync
                                        (configs.SlaveID, startAddress, 
                                        (ushort)numberOfPoints)
                                        .ConfigureAwait(false);
                                    break;
                                case "InputRegister":
                                    modbusData = await currentRequester
                                        .ReadInputRegistersAsync
                                        (configs.SlaveID, startAddress, 
                                        (ushort)numberOfPoints)
                                        .ConfigureAwait(false);
                                    break;
                                case "Coil":
                                    modbusData = await currentRequester
                                        .ReadCoilsAsync
                                        (configs.SlaveID, startAddress, 
                                        (ushort)numberOfPoints)
                                        .ConfigureAwait(false);
                                    break;
                                case "HoldingRegister":
                                    modbusData = await currentRequester
                                        .ReadHoldingRegistersAsync
                                        (configs.SlaveID, startAddress, 
                                        (ushort)numberOfPoints)
                                        .ConfigureAwait(false);
                                    break;
                                default:
                                    continue;
                            }
                            bool dataChanged = true;
                            if (modbusData != null)
                            {
                                if (lastKnownData
                                    .TryGetValue
                                    (dataTypeKey, out var previousData))
                                {
                                    if (previousData != null 
                                        && previousData.Length 
                                        == modbusData.Length)
                                    {
                                        if (modbusData is 
                                            bool[] currentBoolData 
                                            && previousData is 
                                            bool[] previousBoolData)
                                        {
                                            dataChanged = !currentBoolData
                                                .SequenceEqual
                                                (previousBoolData);
                                        }
                                        else if (modbusData is 
                                            ushort[] currentUshortData 
                                            && previousData is 
                                            ushort[] previousUshortData)
                                        {
                                            dataChanged = !currentUshortData
                                                .SequenceEqual
                                                (previousUshortData);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                dataChanged = false;
                            }
                            if (dataChanged)
                            {
                                LogMessage.AppendLogMessage
                                    ($"{dataTypeKey.PadRight(16)}：" + 
                                    (modbusData is Array array ? 
                                    string.Join(", ", array.Cast<object>()) : 
                                    string.Empty));
                                // 更新上次已知数据
                                lastKnownData[dataTypeKey] = modbusData!;
                                // 处理数据转换和更新
                                ObservableCollection<ModbusItems>? 
                                    targetCollection = null;

                                if (dataTypeKey == "DiscreteInput" 
                                    || dataTypeKey == "InputRegister" 
                                    || dataTypeKey == "WriteMultipleCoils" 
                                    || dataTypeKey == "WriteMultipleRegisters")
                                {
                                    targetCollection = inputItems;
                                }
                                else if (dataTypeKey == "Coil" 
                                    || dataTypeKey == "HoldingRegister")
                                {
                                    targetCollection = outputItems;
                                }

                                if (targetCollection != null)
                                {
                                    uiContext.Post(state =>
                                    {
                                        PollingMethod
                                        .UpdateItemsFromModbusData
                                        (targetCollection, dataTypeKey, 
                                        startAddress, modbusData);
                                    }, null);
                                }
                            }
                        }
                        catch (Exception modbusReadEx)
                        {
                            LogMessage.AppendLogMessage
                                ($"Modbus 读取错误，请断开重新连接: " +
                                $"{modbusReadEx.Message}");
                            await Task.Delay(options.WriteTimeoutMs,
                                cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    LogMessage.AppendLogMessage("连接丢失，正在退出连接");
                    break;
                }
                catch (SocketException sockEx)
                {
                    LogMessage.AppendLogMessage($"服务端异常，连接已断开: " +
                        $"{sockEx.Message}");
                    lock (_lock)
                    {
                        IsConnected = false;
                    }
                    ErrorOccurred?.Invoke(this, new Exception
                        ("Socket Error", 
                        sockEx));
                    break;
                }
                catch (Exception ex)
                {
                    LogMessage.AppendLogMessage($"未预料的异常: " +
                        $"{ex.Message}");
                    lock (_lock)
                    {
                        IsConnected = false;
                    }
                    ErrorOccurred?.Invoke(this, new Exception
                        ("Unexpected Error", ex));
                    break;
                }
                await Task.Delay(options.PollIntervalMs, 
                    cancellationToken).ConfigureAwait(false);
            }
        }
        catch (TaskCanceledException)
        {
            LogMessage.AppendLogMessage("任务被取消，正常退出循环");
        }
        catch (Exception ex)
        {
            LogMessage.AppendLogMessage($"通信外部发生异常: {ex.Message}");
            lock (_lock)
            {
                IsConnected = false;
            }
            ErrorOccurred?.Invoke(this, new Exception
                ("Unexpected Error.", ex));
        }
        finally
        {
            LogMessage.AppendLogMessage("通信正常退出");
        }
    }
    #endregion ***** 连接创建和轮询逻辑 *****

    #region ***** 连接断开逻辑 *****
    public void Disconnect()
    {
        lock (_lock)
        {
            if (!_isConnected)
            {
                return;
            }
            IsConnected = false;
            _cts?.Cancel();
        }
    }
    private void CleanupResources()
    {
        lock (_lock)
        {
            _master?.Dispose();
            _master = null;
            _tcpClient?.Close();
            _tcpClient = null;
            _requester = null;
            _cts?.Dispose();
            _cts = null;
            _communicationTask = null;
        }
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lock)
            {
                IsConnected = false;
                _cts?.Cancel();
                try
                {
                    _communicationTask?.Wait(TimeSpan.FromSeconds(10));
                }
                catch (AggregateException ae)
                {
                    foreach (var ex in ae.Flatten().InnerExceptions)
                    {
                        ErrorOccurred?.Invoke(this, 
                            new Exception("", ex));
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, 
                        new Exception("", ex));
                }
                CleanupResources();
            }
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion ***** 连接断开逻辑 *****
}