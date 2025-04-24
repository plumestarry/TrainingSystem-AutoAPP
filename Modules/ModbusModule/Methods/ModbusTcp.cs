using ModbusModule.Methods.Interface;
using ModbusModule.Models;
using NModbus;
using NModbus.Device;
using System.Net.Sockets;

namespace ModbusModule.Methods
{
    /// <summary>
    /// 负责管理 Modbus TCP 连接、状态和事件。
    /// </summary>
    public class ModbusTcp(ModbusOptions options) : IModbusTcp, IDisposable
    {
        private readonly ModbusOptions _options = options ?? throw new ArgumentNullException(nameof(options));
        private TcpClient? _tcpClient;
        private IModbusMaster? _master;
        private Task? _communicationTask;
        private CancellationTokenSource? _cts;
        private bool _isConnected;

        public ModbusTcp() : this(new ModbusOptions())
        {
        }

        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    ConnectionStatusChanged?.Invoke(this, _isConnected);
                }
            }
        }

        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler<Exception>? ErrorOccurred;

        public IModbusRequester Requester { get; private set; }

        public async Task ConnectAsync(ModbusOptions options)
        {
            if (IsConnected)
            {
                return;
            }

            _cts = new CancellationTokenSource();
            var cancellationToken = _cts.Token;

            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(options.IPAddress, options.Port, cancellationToken).ConfigureAwait(false);
                _tcpClient.ReceiveTimeout = options.ReadTimeoutMs;
                _tcpClient.SendTimeout = options.WriteTimeoutMs;

                var factory = new ModbusFactory();
                _master = factory.CreateMaster(_tcpClient);

                Requester = new ModbusRequester(() =>
                {
                    if (!IsConnected || _master == null)
                    {
                        throw new InvalidOperationException("Modbus TCP is not connected.");
                    }
                    return _master;
                });

                IsConnected = true;

                // 启动后台通信任务（这里可以根据你的需求添加实际的通信逻辑，例如轮询读取数据等）
                _communicationTask = Task.Run(async () => await CommunicationLoop(cancellationToken), cancellationToken);
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ErrorOccurred?.Invoke(this, ex);
                // 确保清理资源
                DisconnectInternal();
                throw;
            }
        }

        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            _cts?.Cancel();
            try
            {
                _communicationTask?.Wait(TimeSpan.FromSeconds(5)); // 等待通信任务结束，可以设置超时
            }
            catch (TaskCanceledException)
            {
                // 任务被取消，这是预期行为
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new Exception("Error while waiting for communication task to stop.", ex));
            }
            finally
            {
                DisconnectInternal();
                IsConnected = false;
            }
        }

        private void DisconnectInternal()
        {
            _master?.Dispose();
            _master = null;
            _tcpClient?.Close();
            _tcpClient = null;
            Requester = null;
            _cts?.Dispose();
            _cts = null;
        }

        private async Task CommunicationLoop(CancellationToken cancellationToken)
        {
            // 在这个循环中实现你的 Modbus 通信逻辑
            // 例如，你可以定期读取某些数据，或者等待接收来自服务器的消息（如果服务器会主动发送数据）。
            // 由于 Modbus TCP 通常是请求/响应模式，所以这里的循环可能主要用于维护连接或执行定期的读取操作。

            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected)
                {
                    // 这里可以添加你的通信逻辑，例如：
                    // 1. 定期读取数据
                    // 2. 处理可能由服务器主动发送的消息 (虽然 Modbus TCP 很少有主动发送)
                    // 3. 检查连接状态，如果连接断开尝试重新连接 (更健壮的做法可能是在需要发送请求时检查连接)

                    // 示例：每隔一段时间读取一些数据
                    // try
                    // {
                    //     if (Requester != null)
                    //     {
                    //         var coils = await Requester.ReadCoilsAsync(_options.SlaveID, 0, 10);
                    //         // 处理读取到的线圈状态
                    //         Console.WriteLine($"Read Coils: {string.Join(",", coils)}");
                    //     }
                    // }
                    // catch (Exception ex)
                    // {
                    //     ErrorOccurred?.Invoke(this, new Exception("Error during communication loop.", ex));
                    //     // 可以考虑在这里处理连接断开的情况，例如尝试重新连接
                    //     IsConnected = false;
                    //     break;
                    // }

                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false); // 1秒延迟
                }
            }
            catch (TaskCanceledException)
            {
                // 任务被取消，正常退出
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ErrorOccurred?.Invoke(this, new Exception("Communication loop encountered an error.", ex));
            }
            finally
            {
                // 确保在循环结束时清理资源（如果需要）
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisconnectInternal();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}