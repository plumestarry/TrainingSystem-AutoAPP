using AutoAPP.Core.Dialogs.Interface;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using ModbusModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.ViewModels.Dialogs
{
    public partial class ConnectViewModel : ObservableObject, IDialogHostAware
    {
        /// <summary>
        /// 这里指的是 "ConnectView" 窗口的名称
        /// </summary>
        public string DialogHostName { get ; set; }

        public ConnectViewModel()
        {

        }

        [ObservableProperty]
        private ModbusConfig modbusConfig;

        [RelayCommand]
        public void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult { Result = ButtonResult.No });
        }

        [RelayCommand]
        public void Save()
        {

            if(ModbusConfig == null) return; // 检查 modbusConfig 是否为 null

            if (ModbusConfig.SlaveID < 0 || // 检查 SlaveID 是否为 0
                ModbusConfig.Port <= 0 ||    // 检查 Port 是否为非正数
                string.IsNullOrWhiteSpace(ModbusConfig.IPAddress) // 检查 IPAddress 是否为空或空白
                ) return;


            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                // 确定时，把编辑的实体返回并且返回 OK
                DialogParameters param = new DialogParameters();
                param.Add("Value", ModbusConfig);
                DialogHost.Close(DialogHostName, new DialogResult { Result = ButtonResult.OK, Parameters = param });
            }   
        }

        public void OnDialogOpend(IDialogParameters parameters)
        {
            // 首先先判断参数中是否存在键值对，如果存在则获取值，否则返回新的实例
            if (parameters.ContainsKey("Value"))
            {
                ModbusConfig = parameters.GetValue<ModbusConfig>("Value");
            }
            else
                ModbusConfig = new ModbusConfig();
        }

    }
}
