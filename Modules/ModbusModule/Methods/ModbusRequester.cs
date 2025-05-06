using ModbusModule.Methods.Interface;
using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Methods
{
    /// <summary>
    /// 初始化 ModbusRequester 的新实例。
    /// </summary>
    /// <param name="masterProvider">
    /// 一个委托 (Function)，当需要执行 Modbus 操作时调用它。
    /// 这个委托应该返回当前有效的 IModbusMaster 实例，
    /// 并在未连接或 Master 无效时抛出异常 (例如 InvalidOperationException)。
    /// </param>
    /// <exception cref="ArgumentNullException">如果 masterProvider 为 null。</exception>
    public class ModbusRequester(Func<IModbusMaster> masterProvider) : IModbusRequester
    {
        // 用于获取当前有效 IModbusMaster 实例的委托
        private readonly Func<IModbusMaster> _masterProvider = masterProvider ?? throw new ArgumentNullException(nameof(masterProvider));

        /// <summary>
        /// 获取当前的 IModbusMaster 实例。如果提供者无法提供（例如未连接），则会抛出异常。
        /// </summary>
        private IModbusMaster GetMaster()
        {
            // 调用委托来获取 Master，委托内部会处理连接检查
            return _masterProvider();
        }

        // --- IModbusRequester 实现 ---

        public Task<bool[]> ReadCoilsAsync(byte unitId, ushort startAddress, ushort numberOfPoints)
        {
            var master = GetMaster(); // 获取 Master
            return master.ReadCoilsAsync(unitId, startAddress, numberOfPoints); // 调用 NModbus 方法
        }

        public Task<bool[]> ReadInputsAsync(byte unitId, ushort startAddress, ushort numberOfPoints)
        {
            var master = GetMaster();
            return master.ReadInputsAsync(unitId, startAddress, numberOfPoints);
        }

        public Task<ushort[]> ReadHoldingRegistersAsync(byte unitId, ushort startAddress, ushort numberOfPoints)
        {
            var master = GetMaster();
            return master.ReadHoldingRegistersAsync(unitId, startAddress, numberOfPoints);
        }

        public Task<ushort[]> ReadInputRegistersAsync(byte unitId, ushort startAddress, ushort numberOfPoints)
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
}
