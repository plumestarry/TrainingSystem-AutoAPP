using AutoAPP.Core.Dialogs.Interface;
using AutoAPP.Core.Extensions;
using AutoAPP.Core.Service.Interface;
using AutoShared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ModbusModule.Methods;
using ModbusModule.Methods.Interface;
using ModbusModule.Models;
using Prism.Ioc;
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

        private readonly IModbusTcp ModbusService;

        private readonly IConfigService service;

        public ModbusViewModel(IModbusTcp ModbusService, IConfigService service, IContainerProvider containerProvider, IDialogHostService dialog) : base(containerProvider)
        {

            this.dialog = dialog;
            // 测试初始化集合
            ModbusConfig = new ModbusConfig() { IPAddress = "127.0.0.1", Port = 502, SlaveID = 1 };
            ModbusTimes = new ModbusTimes();

            this.ModbusService = ModbusService;
            this.service = service;

            LogMessage = new LogMessage(ConnectionString);
            LogMessage.AppendLogMessage("初始化完成！");

        }

        #region ****************************** 连接逻辑代码 ******************************

        [ObservableProperty]
        public StatisticViewModel? chart = null;

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string connectButtonText = "连接";

        [RelayCommand]
        void Connect() 
        {
            if (ConnectButtonText == "连接")
            {
                SynchronizationContext? uiContext = SynchronizationContext.Current;
                Chart = new StatisticViewModel(OutputItems);
                WeakReferenceMessenger.Default.Send(new CreatedMessage(Chart, ModbusService));
                ModbusService.ConnectAsync(ModbusConfig, ModbusTimes, LogMessage, InputItems, OutputItems, uiContext);
                ConnectButtonText = "断开";
            }
            else
            {
                Chart?.Dispose();
                Chart = null;
                ModbusService.Disconnect();
                ConnectButtonText = "连接";
            }
        }


        #endregion ****************************** 连接逻辑代码 ******************************

        #region ****************************** 信息配置代码 ******************************

        [ObservableProperty]
        private ModbusConfig modbusConfig;

        [ObservableProperty]
        private ModbusTimes modbusTimes;

        [RelayCommand]
        async Task Config()
        {
            await GetConfig();
            DialogParameters param = new DialogParameters();
            if (ModbusConfig != null)
                param.Add("Value", ModbusConfig);

            var dialogResult = await dialog.ShowDialog("ConnectView", param);
            if (dialogResult.Result == ButtonResult.OK)
            {
                ModbusConfig = dialogResult.Parameters.GetValue<ModbusConfig>("Value");
                await UpdateConfig();
                LogMessage.AppendLogMessage("ModbusConfig 配置成功！");
            }
        }

        async Task GetConfig()
        {
            if (AppSession.UserName == "Guest")
                return;
            try
            {
                UpdateLoading(true);
                var configResult = await service.GetFirstOfDefaultAsync(AppSession.UserName);
                if (configResult.Status)
                {
                    if (configResult.Result != null)
                    {
                        ModbusConfig.IPAddress = configResult.Result.IPAddress;
                        ModbusConfig.Port = configResult.Result.Port;
                        ModbusConfig.SlaveID = configResult.Result.SlaveID;
                    }
                    else
                    {
                        await service.AddAsync(new ConfigDto()
                        {
                            UserName = AppSession.UserName,
                            IPAddress = ModbusConfig.IPAddress,
                            Port = ModbusConfig.Port,
                            SlaveID = ModbusConfig.SlaveID,
                            CreateDate = DateTime.Now,
                        });
                    }
                }
            }
            catch (Exception)
            {
                UpdateLoading(false);
                aggregator.SendMessage("获取配置错误！");
            }
            finally
            {
                UpdateLoading(false);
            }
        }

        async Task UpdateConfig()
        {
            if (AppSession.UserName == "Guest")
                return;
            try
            {
                UpdateLoading(true);
                await service.UpdateAsync(new ConfigDto()
                {
                    UserName = AppSession.UserName,
                    IPAddress = ModbusConfig.IPAddress,
                    Port = ModbusConfig.Port,
                    SlaveID = ModbusConfig.SlaveID,
                });
            }
            catch (Exception)
            {
                UpdateLoading(false);
                aggregator.SendMessage("更新配置错误！");
            }
            finally
            {
                UpdateLoading(false);
            }
        }

        #endregion ****************************** 信息配置代码 ******************************

        #region ****************************** 消息日志滚动配置代码 ******************************

        [ObservableProperty]
        private LogMessage logMessage;

        #endregion ****************************** 消息日志滚动配置代码 ******************************

    }

}
