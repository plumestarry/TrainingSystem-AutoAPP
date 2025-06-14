using ModbusModule.Methods.Interface;
using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ModbusModule.Methods;
public class ModbusRequester(Func<IModbusMaster> masterProvider) : IModbusRequester
{
    // 用于获取当前有效 IModbusMaster 实例的委托
    private readonly Func<IModbusMaster> _masterProvider = 
        masterProvider ?? throw new ArgumentNullException(nameof(masterProvider));
    // 获取当前的 IModbusMaster 实例
    private IModbusMaster GetMaster()
    {
        // 调用委托来获取 Master，委托内部会处理连接检查
        return _masterProvider();
    }
    // IModbusRequester 实现
    public Task<bool[]> ReadCoilsAsync(byte unitId, ushort startAddress, ushort numberOfPoints)
    {
        var master = GetMaster();
        return master.ReadCoilsAsync(unitId, startAddress, numberOfPoints);
    }
    public Task<bool[]> ReadInputsAsync(byte unitId, ushort startAddress, ushort numberOfPoints)
    {
        var master = GetMaster();
        return master.ReadInputsAsync(unitId, startAddress, numberOfPoints);
    }
    public Task<ushort[]> ReadHoldingRegistersAsync
        (byte unitId, ushort startAddress, ushort numberOfPoints)
    {
        var master = GetMaster();
        return master.ReadHoldingRegistersAsync(unitId, startAddress, numberOfPoints);
    }
    public Task<ushort[]> ReadInputRegistersAsync
        (byte unitId, ushort startAddress, ushort numberOfPoints)
    {
        var master = GetMaster();
        return master.ReadInputRegistersAsync(unitId, startAddress, numberOfPoints);
    }
    public Task WriteSingleCoilAsync(byte unitId, ushort startAddress, bool values)
    {
        var master = GetMaster();
        return master.WriteSingleCoilAsync(unitId, startAddress, values);
    }
    public Task WriteSingleRegisterAsync(byte unitId, ushort startAddress, ushort values)
    {
        var master = GetMaster();
        return master.WriteSingleRegisterAsync(unitId, startAddress, values);
    }
    public Task WriteMultipleCoilsAsync(byte unitId, ushort startAddress, bool[] values)
    {
        var master = GetMaster();
        return master.WriteMultipleCoilsAsync(unitId, startAddress, values);
    }
    public Task WriteMultipleRegistersAsync(byte unitId, ushort startAddress, ushort[] values)
    {
        var master = GetMaster();
        return master.WriteMultipleRegistersAsync(unitId, startAddress, values);
    }
}

