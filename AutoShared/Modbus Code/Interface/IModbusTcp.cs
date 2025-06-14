using ModbusModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ModbusModule.Methods.Interface;
public interface IModbusTcp
{
    // 使用提供的配置异步连接到 Modbus TCP 服务器。
    Task ConnectAsync(ModbusConfig configs, ModbusTimes options, 
        LogMessage LogMessage,
        ObservableCollection<ModbusItems> InputItems, 
        ObservableCollection<ModbusItems> OutputItems,
        SynchronizationContext uiContext);
    // 断开与 Modbus TCP 服务器的连接。
    void Disconnect();
    // 获取当前的连接状态
    bool IsConnected { get; }
    // 连接状态改变时触发的事件
    event EventHandler<bool> ConnectionStatusChanged;
    // 通信过程中发生错误时触发的事件
    event EventHandler<Exception> ErrorOccurred;
    // 获取用于执行 Modbus 请求的接口
    IModbusRequester Requester { get; }
    // Modbus 通信参数配置实体属性
    public ModbusConfig? ModbusConfig { get; set; }
}
