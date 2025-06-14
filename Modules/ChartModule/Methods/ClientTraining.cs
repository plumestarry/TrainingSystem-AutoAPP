using AutoAPP.Core.Extensions;
using AutoAPP.Core.Service.Interface;
using ChartModule.Models;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using ModbusModule.Methods.Interface;
using ModbusModule.Models;
using ModbusModule.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartModule.Methods
{
    public static class ClientTraining
    {
        public static void TrainingInit(StatisticViewModel StatisticData, ObservableCollection<ClientItem> ClientItems)
        {
            var count = 0;
            foreach (var item in StatisticData.ChartData)
            {
                if (item.ModbusType == "Register") // 假设 ModbusType 是一个枚举，且 Register 是其中的一个值
                {
                    ClientItems.Add(new ClientItem() { ItemColor = GetColor(StatisticData.Colors[count]), ItemLabel = item.Name.PadRight(8) + "：", ItemValue = 0 });
                    count++;
                }
            }
        }

        public static string GetColor(SKColor color)
        {
            // 构造 16 进制字符串
            return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
        }

        public static void GetPort(StatisticViewModel StatisticData, Dictionary<string, ushort> TrainingPort)
        {
            foreach (var item in StatisticData.ChartData)
            {
                if (item.Name == "Emit")
                {
                    TrainingPort.Add(item.Name, item.Port);
                }
            }

            var port = GetMaxPort(StatisticData);
            TrainingPort.Add("Base", (ushort)(port + 1));
            TrainingPort.Add("Part", (ushort)(port + 2));
        }

        public static ushort GetMaxPort(StatisticViewModel StatisticData)
        {
            return StatisticData.ChartData
                .Where(item => item.ModbusType == "Register")
                .Select(item => item.Port)
                .Max();
        }

        public static async Task TrainingSort1(StatisticViewModel StatisticData, ObservableCollection<ClientItem> ClientItems,
            Dictionary<string, ushort> TrainingPort, IModbusRequester Requester, ModbusConfig ModbusConfig,
            IEventAggregator aggregator, IRecordService service, string title)
        {
            // 使用一个 Random 实例，避免快速连续创建导致随机性不足
            var random = new Random();

            var numOfTraining = 1;

            // 定义需要发送的数据值
            ushort[] dataValues = { 0x0020, 0x0100, 0x0800, 0x0010, 0x0080, 0x0400 };

            // 定义颜色掩码
            ushort GreenMask = 0x0920; // 绿色： 0x0020 0x0100 0x0800 (0x0020 | 0x0100 | 0x0800 的超集，用于判断)
            ushort BlueMask = 0x0490;  // 蓝色： 0x0010 0x0080 0x0400 (0x0010 | 0x0080 | 0x0400 的超集，用于判断)
                                       // 注意：根据提供的数值和掩码，每个数值只会匹配一个颜色。
                                       // 0x0020, 0x0100, 0x0800 都与 GreenMask 相与大于 0。
                                       // 0x0010, 0x0080, 0x0400 都与 BlueMask 相与大于 0。

            // 定义各轮实训的参数：循环次数和延迟时间（毫秒）
            var rounds = new[]
            {
                new { Count = 4, Delay = 6000 }, // 第一轮
                new { Count = 5, Delay = 5000 }, // 第二轮
                new { Count = 10, Delay = 4000 } // 第三轮
            };

            // 初始延迟 5 秒并复位 Base 寄存器
            // 注意：Task.Delay(...).Wait() 是同步阻塞的，会阻塞当前线程直到延迟结束。
            // 如果此方法在 UI 线程上调用，会导致 UI 无响应。更常见的异步模式是使用 async/await。
            // 但为了与原代码的 Wait() 调用风格保持一致，这里也使用 Wait()。
            await Task.Delay(5000);
            await Requester.WriteSingleRegisterAsync(ModbusConfig.SlaveID, TrainingPort["Base"], 0);

            // 查找需要更新的 ClientItem (假设 ItemLabel 唯一)
            // 假设 ClientItem 有一个公共的 int 属性用于计数，例如 Count
            var greenItem = ClientItems.FirstOrDefault(item => item.ItemLabel.Contains("Green"));
            var blueItem = ClientItems.FirstOrDefault(item => item.ItemLabel.Contains("Blue"));

            var greenRegister = StatisticData.ChartData.FirstOrDefault(item => item.Name.Contains("Green"));
            var blueRegister = StatisticData.ChartData.FirstOrDefault(item => item.Name.Contains("Blue"));

            if (greenRegister.Status > 0 && blueRegister.Status > 0)
            {
                aggregator.SendMessage("寄存器未复位，请复位后再开始实训");
                return;
            }

            // 循环执行各轮实训
            foreach (var round in rounds)
            {
                for (int i = 0; i < round.Count; i++)
                {
                    if (!AppTraining.IsTraining)
                    {
                        return;
                    }
                    // 随机选择一个数据值
                    ushort data = dataValues[random.Next(dataValues.Length)];

                    // 发送 Part 寄存器数据
                    await Task.Delay(50);
                    await Requester.WriteSingleRegisterAsync(ModbusConfig.SlaveID, TrainingPort["Part"], data);

                    // 根据发送的数据更新 ClientItems
                    // 使用 (data & Mask) > 0 的形式确保位与操作的优先级
                    if ((data & GreenMask) > 0)
                    {
                        // 如果找到绿色对应的 ClientItem，则更新其计数
                        if (greenItem != null)
                        {
                            // 假设 ClientItem 有一个 Count 属性，且 ObservableCollection 能够处理属性变更通知
                            greenItem.ItemValue++;
                        }
                    }
                    // 使用 else if 确保一个数据不会被同时判定为绿色和蓝色（根据掩码和数据值，这是互斥的）
                    else if ((data & BlueMask) > 0)
                    {
                        // 如果找到蓝色对应的 ClientItem，则更新其计数
                        if (blueItem != null)
                        {
                            // 假设 ClientItem 有一个 Count 属性
                            blueItem.ItemValue++;
                        }
                    }
                    // 如果数据值不匹配任何一个掩码，则不更新任何 ClientItem。
                    // 根据提供的 dataValues 和 Mask，这种情况不会发生。

                    // 发送 Emit 线圈信号，启动分拣过程
                    // 此操作应在发送数据后进行，无论数据代表何种颜色
                    await Task.Delay(50);
                    await Requester.WriteSingleCoilAsync(ModbusConfig.SlaveID, TrainingPort["Emit"], true);

                    // 等待指定的延迟时间
                    await Task.Delay(round.Delay);
                    await Requester.WriteSingleCoilAsync(ModbusConfig.SlaveID, TrainingPort["Emit"], false);
                }

                await Task.Delay(15000); // 等待 10 秒，可能是为了观察结果或进行其他操作

                var ifPassed = greenItem.ItemValue == greenRegister.Status && blueItem.ItemValue == blueRegister.Status;

                if (ifPassed)
                {
                    aggregator.SendMessage($"第{numOfTraining}轮实训完成");
                    if (numOfTraining == 3)
                    {
                        await UploadGrade(aggregator, service, title, numOfTraining.ToString());
                        return;
                    }
                }
                else
                {
                    aggregator.SendMessage($"第{numOfTraining}轮实训失败，请停止实训");
                    numOfTraining--;
                    await UploadGrade(aggregator, service, title, numOfTraining.ToString());
                    return;
                }

                numOfTraining ++;
            }
        }

        public static async Task UploadGrade(IEventAggregator aggregator, IRecordService service, string title, string content)
        {
            if (AppSession.UserName == "Guest")
                return;
            try
            {
                await service.AddAsync(new AutoShared.Dtos.RecordDto()
                {
                    UserName = AppSession.UserName,
                    Title = title,
                    Content = content,
                    CreateDate = DateTime.Now,
                });
                aggregator.SendMessage($"正在上传实训记录");
            }
            catch (Exception)
            {
                aggregator.SendMessage($"实训记录上传失败");
            }
        }

        public static async Task TrainingSort2(StatisticViewModel StatisticData, ObservableCollection<ClientItem> ClientItems,
            Dictionary<string, ushort> TrainingPort, IModbusRequester Requester, ModbusConfig ModbusConfig,
            IEventAggregator aggregator, IRecordService service, string title)
        {
            // 使用一个 Random 实例，避免快速连续创建导致随机性不足
            var random = new Random();

            var numOfTraining = 1;

            // 定义需要发送的数据值
            ushort[] dataValues = { 0x0080, 0x0100, 0x0200, 0x0400, 0x0800, 0x1000 };

            // 定义颜色
            ushort GreenMask = 0x0100; // 绿色
            ushort BlueMask = 0x0080;  // 蓝色
            ushort MetalMask = 0x0200;  // 金属

            // 定义各轮实训的参数：循环次数和延迟时间（毫秒）
            var rounds = new[]
            {
                new { Count = 5, Delay = 5000 }, // 第一轮
                new { Count = 5, Delay = 4000 }, // 第二轮
                new { Count = 20, Delay = 3000 } // 第三轮
            };

            // 初始延迟 5 秒并复位 Base 寄存器
            // 注意：Task.Delay(...).Wait() 是同步阻塞的，会阻塞当前线程直到延迟结束。
            // 如果此方法在 UI 线程上调用，会导致 UI 无响应。更常见的异步模式是使用 async/await。
            // 但为了与原代码的 Wait() 调用风格保持一致，这里也使用 Wait()。
            await Task.Delay(5000);
            await Requester.WriteSingleRegisterAsync(ModbusConfig.SlaveID, TrainingPort["Base"], 0);

            // 查找需要更新的 ClientItem (假设 ItemLabel 唯一)
            // 假设 ClientItem 有一个公共的 int 属性用于计数，例如 Count
            var greenItem = ClientItems.FirstOrDefault(item => item.ItemLabel.Contains("Green"));
            var blueItem = ClientItems.FirstOrDefault(item => item.ItemLabel.Contains("Blue"));
            var metalItem = ClientItems.FirstOrDefault(item => item.ItemLabel.Contains("Metal"));
            var otherItem = ClientItems.FirstOrDefault(item => item.ItemLabel.Contains("Other"));

            var greenRegister = StatisticData.ChartData.FirstOrDefault(item => item.Name.Contains("Green"));
            var blueRegister = StatisticData.ChartData.FirstOrDefault(item => item.Name.Contains("Blue"));
            var metalRegister = StatisticData.ChartData.FirstOrDefault(item => item.Name.Contains("Metal"));
            var otherRegister = StatisticData.ChartData.FirstOrDefault(item => item.Name.Contains("Other"));

            if (greenRegister.Status > 0 || blueRegister.Status > 0 || metalRegister.Status > 0 || otherRegister.Status > 0)
            {
                aggregator.SendMessage("寄存器未复位，请复位后再开始实训");
                return;
            }

            // 循环执行各轮实训
            foreach (var round in rounds)
            {
                for (int i = 0; i < round.Count; i++)
                {
                    if (!AppTraining.IsTraining)
                    {
                        return;
                    }
                    // 随机选择一个数据值
                    ushort data = dataValues[random.Next(dataValues.Length)];

                    // 发送 Part 寄存器数据
                    await Task.Delay(50);
                    await Requester.WriteSingleRegisterAsync(ModbusConfig.SlaveID, TrainingPort["Part"], data);

                    // 根据发送的数据更新 ClientItems
                    // 使用 (data & Mask) > 0 的形式确保位与操作的优先级
                    if (data == GreenMask)
                    {
                        // 如果找到绿色对应的 ClientItem，则更新其计数
                        if (greenItem != null)
                        {
                            // 假设 ClientItem 有一个 Count 属性，且 ObservableCollection 能够处理属性变更通知
                            greenItem.ItemValue++;
                        }
                    }
                    // 使用 else if 确保一个数据不会被同时判定为绿色和蓝色（根据掩码和数据值，这是互斥的）
                    else if (data == BlueMask)
                    {
                        // 如果找到蓝色对应的 ClientItem，则更新其计数
                        if (blueItem != null)
                        {
                            // 假设 ClientItem 有一个 Count 属性
                            blueItem.ItemValue++;
                        }
                    }
                    else if (data == MetalMask)
                    {
                        // 如果找到金属对应的 ClientItem，则更新其计数
                        if (metalItem != null)
                        {
                            // 假设 ClientItem 有一个 Count 属性
                            metalItem.ItemValue++;
                        }
                    }
                    else if (data > 0)
                    {
                        // 如果找到其他对应的 ClientItem，则更新其计数
                        if (otherItem != null)
                        {
                            // 假设 ClientItem 有一个 Count 属性
                            otherItem.ItemValue++;
                        }
                    }
                    // 如果数据值不匹配任何一个掩码，则不更新任何 ClientItem。
                    // 根据提供的 dataValues 和 Mask，这种情况不会发生。

                    // 发送 Emit 线圈信号，启动分拣过程
                    // 此操作应在发送数据后进行，无论数据代表何种颜色
                    await Task.Delay(50);
                    await Requester.WriteSingleCoilAsync(ModbusConfig.SlaveID, TrainingPort["Emit"], true);

                    // 等待指定的延迟时间
                    await Task.Delay(round.Delay);
                    await Requester.WriteSingleCoilAsync(ModbusConfig.SlaveID, TrainingPort["Emit"], false);
                }

                await Task.Delay(15000); // 等待 10 秒，可能是为了观察结果或进行其他操作

                var ifPassed = 
                    greenItem.ItemValue == greenRegister.Status && 
                    blueItem.ItemValue == blueRegister.Status &&
                    metalItem.ItemValue == metalRegister.Status &&
                    otherItem.ItemValue == otherRegister.Status;

                if (ifPassed)
                {
                    aggregator.SendMessage($"第{numOfTraining}轮实训完成");
                    if (numOfTraining == 3)
                    {
                        await UploadGrade(aggregator, service, title, numOfTraining.ToString());
                        return;
                    }
                }
                else
                {
                    aggregator.SendMessage($"第{numOfTraining}轮实训失败，请停止实训");
                    numOfTraining--;
                    await UploadGrade(aggregator, service, title, numOfTraining.ToString());
                    return;
                }

                numOfTraining++;
            }
        }
    }
}