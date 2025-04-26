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
    }
}
