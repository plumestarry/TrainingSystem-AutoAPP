using ModbusModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Methods
{
    public class PollingMethod
    {

        public static Dictionary<string, (ushort MinPort, ushort MaxPort)> GetPortRanges(ObservableCollection<ModbusItems> inputItems,
            ObservableCollection<ModbusItems> outputItems)
        {
            var result = new Dictionary<string, (ushort MinPort, ushort MaxPort)>();

            // 处理 modbusType 为 "Bool"
            var discreteInput = inputItems.Where(item => item.ModbusType == "Bool").Select(item => item.Port);

            if (discreteInput.Any())
            {
                result["DiscreteInput"] = (discreteInput.Min(), discreteInput.Max());
            }

            var coil = outputItems.Where(item => item.ModbusType == "Bool").Select(item => item.Port);

            if (coil.Any())
            {
                result["Coil"] = (coil.Min(), coil.Max());
            }

            // 处理 modbusType 为 "Register"
            var inputRegister = inputItems.Where(item => item.ModbusType == "Register").Select(item => item.Port);

            if (inputRegister.Any())
            {
                result["InputRegister"] = (inputRegister.Min(), inputRegister.Max());
            }

            var holdingRegister = outputItems.Where(item => item.ModbusType == "Register").Select(item => item.Port);

            if (holdingRegister.Any())
            {
                result["HoldingRegister"] = (holdingRegister.Min(), holdingRegister.Max());
            }

            return result;
        }


        /// <summary>
        /// 根据接收到的 Modbus 数据更新 ObservableCollection<ModbusItems> 中对应项的状态 (Status)。
        /// 这个方法应该在 UI 线程上调用，因为 targetCollection 绑定到 UI 控件。
        /// </summary>
        /// <param name="targetCollection">要更新的目标集合 (InputItems 或 OutputItems)，已由调用方确定。</param>
        /// <param name="dataTypeKey">数据的类型键 (例如 "DiscreteInput", "Coil", "InputRegister", "HoldingRegister")，用于指导数据解析和项筛选。</param>
        /// <param name="startAddress">接收到的数据对应的 Modbus 起始地址。</param>
        /// <param name="modbusData">从 Modbus 读取到的原始数据 (通常是 byte[], 也可能支持 bool[] 或 ushort[])。</param>
        public static void UpdateItemsFromModbusData(
            ObservableCollection<ModbusItems> targetCollection,
            string dataTypeKey,
            int startAddress,
            object? modbusData)
        {
            if (targetCollection == null || modbusData == null)
            {
                // 没有集合或数据，无需更新
                return;
            }

            // ** 确定预期的 ModbusItems.ModbusType 和数据的解析方式 **
            // 不再判断 targetCollection，只用 dataTypeKey 来指导解析和筛选
            string expectedModbusType;
            bool isBoolType;

            switch (dataTypeKey)
            {
                case "DiscreteInput":
                case "Coil":
                // 如果你的读取逻辑返回这些类型的写入确认，也按对应的读取类型处理
                case "WriteMultipleCoils":
                    expectedModbusType = "Bool";
                    isBoolType = true;
                    break;
                case "InputRegister":
                case "HoldingRegister":
                // 如果你的读取逻辑返回这些类型的写入确认，也按对应的读取类型处理
                case "WriteMultipleRegisters":
                    expectedModbusType = "Register";
                    isBoolType = false;
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"Warning: Unknown or unhandled dataTypeKey '{dataTypeKey}'. Cannot process data.");
                    return;
            }

            // ** 获取目标集合中匹配预期 ModbusType 的项 **
            // 直接对传入的 targetCollection 进行筛选
            var itemsToUpdate = targetCollection.Where(item => item.ModbusType == expectedModbusType).ToList();

            if (!itemsToUpdate.Any())
            {
                // 集合中没有匹配预期类型的项，无需更新
                // System.Diagnostics.Debug.WriteLine($"Debug: No items of type '{expectedModbusType}' found in target collection for dataTypeKey '{dataTypeKey}'.");
                return;
            }

            // ** 根据数据类型解析数据并更新项的状态 **
            if (isBoolType) // 处理布尔类型 (DiscreteInput, Coil)
            {
                bool[] boolValues = null;

                // --- 数据解析逻辑 (与之前相同，这里不再重复写具体实现，保持框架) ---
                // 根据 modbusData 的实际类型 (byte[], bool[], etc.) 进行解析
                if (modbusData is byte[] byteData)
                {
                    // 从字节数组解析位
                    int numberOfBits = byteData.Length * 8;
                    boolValues = new bool[numberOfBits];
                    for (int i = 0; i < numberOfBits; i++)
                    {
                        boolValues[i] = (byteData[i / 8] & (1 << (i % 8))) != 0;
                    }
                }
                else if (modbusData is bool[] directBoolData)
                {
                    boolValues = directBoolData;
                }
                else if (modbusData is Array array && array.Length > 0 && array.GetValue(0) is bool)
                {
                    boolValues = array.Cast<bool>().ToArray();
                }
                // --- 数据解析逻辑结束 ---

                if (boolValues == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Failed to parse data for Bool type '{dataTypeKey}'. Unexpected modbusData type or format.");
                    return; // 解析失败
                }


                // ** 更新集合中匹配项的状态 **
                foreach (var item in itemsToUpdate) // 遍历筛选出的项
                {
                    int dataIndex = item.Port - startAddress; // 数据在解析后数组中的索引

                    if (dataIndex >= 0 && dataIndex < boolValues.Length)
                    {
                        // Bool 值转换为 Status (0 或 1)
                        ushort newStatus = boolValues[dataIndex] ? (ushort)1 : (ushort)0;

                        // 仅在状态改变时更新
                        if (item.Status != newStatus)
                        {
                            item.Status = newStatus;
                        }
                    }
                    // else: item 的 Port 超出了当前接收到的数据范围，忽略
                }
            }
            else // 处理寄存器类型 (InputRegister, HoldingRegister)
            {
                ushort[] registerValues = null;

                // --- 数据解析逻辑 (与之前相同，这里不再重复写具体实现，保持框架) ---
                // 根据 modbusData 的实际类型 (byte[], ushort[], etc.) 进行解析
                if (modbusData is byte[] byteData)
                {
                    if (byteData.Length % 2 != 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Byte array length ({byteData.Length}) is not a multiple of 2 for Register type '{dataTypeKey}'.");
                        return;
                    }
                    registerValues = new ushort[byteData.Length / 2];
                    for (int i = 0; i < registerValues.Length; i++)
                    {
                        // 注意大小端序问题，如果需要手动处理则在这里添加字节反转
                        registerValues[i] = BitConverter.ToUInt16(byteData, i * 2);
                    }
                }
                else if (modbusData is ushort[] directUshortData)
                {
                    registerValues = directUshortData;
                }
                else if (modbusData is Array array && array.Length > 0 && array.GetValue(0) is ushort)
                {
                    registerValues = array.Cast<ushort>().ToArray();
                }
                // --- 数据解析逻辑结束 ---

                if (registerValues == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Failed to parse data for Register type '{dataTypeKey}'. Unexpected modbusData type or format.");
                    return; // 解析失败
                }


                // ** 更新集合中匹配项的状态 **
                foreach (var item in itemsToUpdate) // 遍历筛选出的项
                {
                    int dataIndex = item.Port - startAddress; // 数据在解析后数组中的索引

                    if (dataIndex >= 0 && dataIndex < registerValues.Length)
                    {
                        ushort newStatus = registerValues[dataIndex];

                        // 仅在状态改变时更新
                        if (item.Status != newStatus)
                        {
                            item.Status = newStatus;
                        }
                    }
                    // else: item 的 Port 超出了当前接收到的数据范围，忽略
                }
            }
        }
    }
}
