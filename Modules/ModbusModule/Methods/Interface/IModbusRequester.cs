using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Methods.Interface
{
    public interface IModbusRequester
    {
        /// <summary>
        /// 读取线圈状态 (功能码 01)。
        /// </summary>
        /// <param name="unitId">单元 ID (从站地址)。</param>
        /// <param name="startAddress">起始地址。</param>
        /// <param name="numberOfPoints">要读取的线圈数量。</param>
        /// <returns>包含线圈值 (布尔数组) 的 Task。</returns>
        /// <exception cref="InvalidOperationException">如果未连接，则抛出此异常。</exception>
        Task<bool[]> ReadCoilsAsync(byte unitId, ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 读取离散输入状态 (功能码 02)。
        /// </summary>
        /// <param name="unitId">单元 ID (从站地址)。</param>
        /// <param name="startAddress">起始地址。</param>
        /// <param name="numberOfPoints">要读取的离散输入数量。</param>
        /// <returns>包含离散输入值 (布尔数组) 的 Task。</returns>
        /// <exception cref="InvalidOperationException">如果未连接，则抛出此异常。</exception>
        Task<bool[]> ReadInputsAsync(byte unitId, ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 读取保持寄存器值 (功能码 03)。
        /// </summary>
        /// <param name="unitId">单元 ID (从站地址)。</param>
        /// <param name="startAddress">起始地址。</param>
        /// <param name="numberOfPoints">要读取的寄存器数量。</param>
        /// <returns>包含寄存器值 (ushort 数组) 的 Task。</returns>
        /// <exception cref="InvalidOperationException">如果未连接，则抛出此异常。</exception>
        Task<ushort[]> ReadHoldingRegistersAsync(byte unitId, ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 读取输入寄存器值 (功能码 04)。
        /// </summary>
        /// <param name="unitId">单元 ID (从站地址)。</param>
        /// <param name="startAddress">起始地址。</param>
        /// <param name="numberOfPoints">要读取的寄存器数量。</param>
        /// <returns>包含寄存器值 (ushort 数组) 的 Task。</returns>
        /// <exception cref="InvalidOperationException">如果未连接，则抛出此异常。</exception>
        Task<ushort[]> ReadInputRegistersAsync(byte unitId, ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 写入多个线圈 (功能码 0F)。
        /// </summary>
        /// <param name="unitId">单元 ID (从站地址)。</param>
        /// <param name="startAddress">起始地址。</param>
        /// <param name="values">要写入的线圈值数组。</param>
        /// <returns>表示异步写入操作的 Task。</returns>
        /// <exception cref="InvalidOperationException">如果未连接，则抛出此异常。</exception>
        Task WriteMultipleCoilsAsync(byte unitId, ushort startAddress, bool[] values);

        /// <summary>
        /// 写入多个保持寄存器 (功能码 10)。
        /// </summary>
        /// <param name="unitId">单元 ID (从站地址)。</param>
        /// <param name="startAddress">起始地址。</param>
        /// <param name="values">要写入的寄存器值数组。</param>
        /// <returns>表示异步写入操作的 Task。</returns>
        /// <exception cref="InvalidOperationException">如果未连接，则抛出此异常。</exception>
        Task WriteMultipleRegistersAsync(byte unitId, ushort startAddress, ushort[] values);

        // 注意：在标准的 Modbus TCP 客户端实现中，"接收消息"通常是指接收对您发送的请求的响应。
        // 上述异步方法 (Task<T>) 就是用来获取这些响应的。如果您需要处理非请求-响应模式下的
        // "接收消息" (例如服务器主动推送数据，但这不属于标准 Modbus TCP 客户端功能)，
        // 则需要更复杂的机制，并且超出了标准功能码的范畴。
    }
}
