using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Methods.Interface;

public interface IModbusRequester
{
    // 读取线圈状态（功能码 01）
    Task<bool[]> ReadCoilsAsync(byte unitId, ushort startAddress, ushort numberOfPoints);
    // 读取离散输入状态（功能码 02）
    Task<bool[]> ReadInputsAsync(byte unitId, ushort startAddress, ushort numberOfPoints);
    // 读取保持寄存器值（功能码 03）
    Task<ushort[]> ReadHoldingRegistersAsync
        (byte unitId, ushort startAddress, ushort numberOfPoints);
    // 读取输入寄存器值（功能码 04）
    Task<ushort[]> ReadInputRegistersAsync
        (byte unitId, ushort startAddress, ushort numberOfPoints);
    // 写入单个线圈（功能码 05）
    Task WriteSingleCoilAsync(byte unitId, ushort startAddress, bool values);
    // 写入单个保持寄存器（功能码 06）
    Task WriteSingleRegisterAsync(byte unitId, ushort startAddress, ushort value);
    // 写入多个线圈（功能码 0F）
    Task WriteMultipleCoilsAsync(byte unitId, ushort startAddress, bool[] values);
    // 写入多个保持寄存器（功能码 10）
    Task WriteMultipleRegistersAsync(byte unitId, ushort startAddress, ushort[] values);
}

