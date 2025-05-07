using AutoAPP.Core.Dialogs.Interface;
using AutoAPP.Core.Extensions;
using AutoAPP.Core.Service.Interface;
using AutoAPP.Core.ViewModels;
using ChartModule.Methods;
using ChartModule.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ModbusModule.Methods;
using ModbusModule.Methods.Interface;
using ModbusModule.Models;
using ModbusModule.ViewModels;
using NModbus.Device;
using Prism.Ioc;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;

namespace ChartModule.ViewModels
{
    public partial class ChartViewModel : NavigationViewModel, IRecipient<CreatedMessage>, IRecipient<PropertyChangedMessage<string>>
    {
        public IModbusTcp ModbusService { get; set; }

        private readonly IRecordService service;

        // 在构造函数中注册消息监听器
        public ChartViewModel(IRecordService service, IContainerProvider containerProvider) : base(containerProvider)
        {
            // 注册监听 StatisticViewModelCreatedMessage 消息
            WeakReferenceMessenger.Default.Register<CreatedMessage>(this);
            WeakReferenceMessenger.Default.Register<PropertyChangedMessage<string>>(this);
            ClientItems = new ObservableCollection<ClientItem>();
            TrainingPort = new Dictionary<string, ushort>();
            this.service = service;

            // 注意：WeakReferenceMessenger 使用弱引用，通常不需要手动注销
            // 但如果你的 ViewModel 生命周期复杂，或者有很多订阅，可以考虑在 Dispose 或 OnNavigatedFrom 中注销
        }

        // 添加一个 ObservableProperty 来持有接收到的 StatisticViewModel 实例
        [ObservableProperty]
        private StatisticViewModel? statisticData;

        [ObservableProperty]
        private string selectedCommunicationEntity = "Sort1";

        [ObservableProperty]
        private string? connectButtonText;

        // 实现 IRecipient<StatisticViewModelCreatedMessage> 接口的 Receive 方法
        // 当收到 StatisticViewModelCreatedMessage 消息时，此方法会被调用
        public void Receive(CreatedMessage message)
        {
            // 从消息中获取 StatisticViewModel 实例
            StatisticData = message.Value;
            ModbusService = message.ModbusService;

            // 此时，StatisticData 属性已经被设置，ChartView 可以绑定到 ChartViewModel.StatisticData
            // 并在 StatisticData 中访问如 OutputItems 等属性来更新图表显示。
            // 例如，如果 StatisticViewModel 有一个 ObservableCollection<DataPoint> 或类似的属性，
            // ChartView 的图表控件就可以直接绑定到 statisticData.DataPoints。
            // 可以在这里添加其他逻辑，例如触发图表刷新等（如果绑定不是自动的）
        }

        // 实现 IRecipient<PropertyChangedMessage<string>> 接口
        // 用于接收 SourceViewModel 的字符串属性改变通知 (通过 [NotifyPropertyChangedRecipients] 自动发送)
        public void Receive(PropertyChangedMessage<string> message)
        {
            // 可以根据消息的 Sender 或 PropertyName 来判断是哪个属性或哪个 ViewModel 发来的
            // 这里简单假设所有 string 属性改变都处理
            if (message.Sender is ModbusViewModel) // 确认是 SourceViewModel 发来的
            {
                //如果需要区分 SourceViewModel 中的不同字符串属性，可以检查 message.PropertyName
                if (message.PropertyName == nameof(ModbusViewModel.SelectedCommunicationEntity))
                {
                    SelectedCommunicationEntity = message.NewValue; // 更新 TargetViewModel 的属性
                }
                if (message.PropertyName == nameof(ModbusViewModel.ConnectButtonText))
                {
                    ConnectButtonText = message.NewValue; // 更新 TargetViewModel 的属性
                }
            }
        }

        // 如果你的 NavigationViewModel 有 Dispose 或 Cleanup 方法，考虑在这里注销消息
        // public override void Destroy() // 示例：Prism的Destroy
        // {
        //     WeakReferenceMessenger.Default.Unregister<StatisticViewModelCreatedMessage>(this);
        //     base.Destroy();
        // }

        [ObservableProperty]
        private bool trainingEnabled = true;

        [ObservableProperty]
        ObservableCollection<ClientItem> clientItems;

        [ObservableProperty]
        Dictionary<string, ushort> trainingPort;

        [RelayCommand]
        void TrainingBegin()
        {
            if (ConnectButtonText == "断开" || StatisticData != null)
            {
                UpdateLoading(true);
                AppTraining.IsTraining = true;
                ClientTraining.TrainingInit(StatisticData, ClientItems);
                ClientTraining.GetPort(StatisticData, TrainingPort);
                switch (SelectedCommunicationEntity)
                {
                    case "Sort1":
                        _ = ClientTraining.TrainingSort1(StatisticData, ClientItems, TrainingPort, 
                            ModbusService.Requester, ModbusService.ModbusConfig, 
                            aggregator, service, "Sort1");
                        break;
                    case "Sort2":
                        break;
                    default:
                        break;
                }
                TrainingEnabled = false;
                UpdateLoading(false);
                aggregator.SendMessage("实训开始进行");
            }
            else
            {
                aggregator.SendMessage("通信未连接，无法开始实训");
            }
        }

        [RelayCommand]
        void TrainingStop()
        {
            UpdateLoading(true);
            AppTraining.IsTraining = false;
            ClientItems.Clear();
            TrainingPort.Clear();
            TrainingEnabled = true;
            UpdateLoading(false);
            aggregator.SendMessage("正在强制停止实训");
        }

    }
}