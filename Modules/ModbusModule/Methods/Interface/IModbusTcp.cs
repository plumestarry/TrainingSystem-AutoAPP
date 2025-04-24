using ModbusModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Methods.Interface
{
    public interface IModbusTcp
    {
        /// <summary>
        /// 使用提供的配置异步连接到 Modbus TCP 服务器。
        /// 内部通信将在单独的线程或任务中处理。
        /// </summary>
        /// <param name="options">Modbus TCP 连接配置选项。</param>
        /// <returns>表示异步连接操作的 Task。</returns>
        Task ConnectAsync(ModbusOptions options);

        /// <summary>
        /// 断开与 Modbus TCP 服务器的连接。
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 获取当前的连接状态。
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 连接状态改变时触发的事件。
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>
        /// 通信过程中发生错误时触发的事件。
        /// </summary>
        event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// 获取用于执行 Modbus 请求的接口。
        /// 只有在连接成功 (IsConnected 为 true) 后才可用。
        /// </summary>
        IModbusRequester Requester { get; }
    }
}
