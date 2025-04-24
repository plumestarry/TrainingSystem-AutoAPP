using AutoAPP.Core.Dialogs.Interface;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModbusModule.Methods;
using ModbusModule.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;



// Modbus通信界面 ViewModel
namespace ModbusModule.ViewModels
{
    public partial class ModbusViewModel : ModbusContentVM
    {
        private readonly IDialogHostService dialog;

        public ModbusViewModel(IContainerProvider containerProvider, IDialogHostService dialog) : base(containerProvider)
        {

            this.dialog = dialog;
            AppendLogMessage("应用程序启动");

        }

        #region ****************************** 连接逻辑代码 ******************************

        [ObservableProperty]
        private bool isConnected;

        [ObservableProperty]
        public string connectButtonText;


        #endregion ****************************** 连接逻辑代码 ******************************

        #region ****************************** 信息配置代码 ******************************

        [ObservableProperty]
        private ModbusConfig modbusConfig;

        [RelayCommand]
        async Task Config()
        {
            DialogParameters param = new DialogParameters();
            if (ModbusConfig != null)
                param.Add("Value", ModbusConfig);

            var dialogResult = await dialog.ShowDialog("ConnectView", param);
            if (dialogResult.Result == ButtonResult.OK)
            {
                try
                {
                    UpdateLoading(true);
                    ModbusConfig = dialogResult.Parameters.GetValue<ModbusConfig>("Value");

                    //if (memo.Id > 0)
                    //{
                    //    var updateResult = await memoService.UpdateAsync(memo);
                    //    if (updateResult.Status)
                    //    {
                    //        var memoModel = summary.MemoList.FirstOrDefault(t => t.Id.Equals(memo.Id));
                    //        if (memoModel != null)
                    //        {
                    //            memoModel.Title = memo.Title;
                    //            memoModel.Content = memo.Content;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    var addResult = await memoService.AddAsync(memo);
                    //    if (addResult.Status)
                    //    {
                    //        summary.MemoList.Add(addResult.Result);
                    //    }
                    //}
                    AppendLogMessage("ModbusConfig 配置成功！");
                }
                finally
                {
                    UpdateLoading(false);
                }
            }
        }

        #endregion ****************************** 信息配置代码 ******************************

        #region ****************************** 消息日志滚动配置代码 ******************************

        // 定义日志保存阈值和移除数量
        private const int LogThreshold = 100; // 达到 100 条时触发保存
        private const int LinesToSaveAndRemove = 50; // 保存并移除前 50 条

        // 用一个 List 来存储所有的日志行
        private readonly List<string> logLines = new List<string>();

        // 绑定到 Textbox 的属性，需要触发 PropertyChanged 通知
        [ObservableProperty]
        private string logMessages = "";

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

        #endregion ****************************** 消息日志滚动配置代码 ******************************

    }

}
