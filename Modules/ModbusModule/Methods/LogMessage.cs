using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Methods
{
    public partial class LogMessage(string ConnectionString) : ObservableObject
    {

        [ObservableProperty]
        public string logMessages = "";

        // 定义日志保存阈值和移除数量
        private const int LogThreshold = 100; // 达到 100 条时触发保存
        private const int LinesToSaveAndRemove = 50; // 保存并移除前 50 条

        // 用一个 List 来存储所有的日志行
        private readonly List<string> logLines = new List<string>();

        public void AppendLogMessage(string message)
        {
            string timestampedMessage = $"{DateTime.Now:HH:mm:ss} - {message}";

            // 将新消息添加到内部列表中
            logLines.Add(timestampedMessage);

            // 检查是否达到保存阈值
            if (logLines.Count >= LogThreshold)
            {
                // 提取需要保存的旧日志（前 LinesToSaveAndRemove 条）
                List<string> linesToSave = logLines.Take(LinesToSaveAndRemove).ToList();

                IODataMethod.SaveModbusData(ConnectionString, linesToSave);

                // 移除已保存的日志行
                logLines.RemoveRange(0, LinesToSaveAndRemove);
            }

            // 重新生成 LogMessages 字符串并更新绑定的属性
            // 使用 Environment.NewLine 确保跨平台兼容换行符
            LogMessages = string.Join(Environment.NewLine, logLines);

        }
    }
}
