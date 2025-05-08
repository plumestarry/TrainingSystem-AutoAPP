using AutoAPP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using hyjiacan.py4n;

namespace AutoAPP.ViewModels
{
    public partial class MainViewModel : ObservableObject, IConfigureService
    {
        private readonly IRecordService recordService;
        private readonly IConfigService configService;

        private readonly IContainerProvider containerProvider;
        private readonly IRegionManager regionManager;

        public MainViewModel(IContainerProvider containerProvider, IRegionManager regionManager)
        {
            MenuBars = new ObservableCollection<MenuBar>();
            this.recordService = containerProvider.Resolve<IRecordService>();
            this.configService = containerProvider.Resolve<IConfigService>();
            this.containerProvider = containerProvider;
            this.regionManager = regionManager;
        }

        [ObservableProperty]
        string userName = "Guest";

        [ObservableProperty]
        char initial;

        [ObservableProperty]
        ObservableCollection<MenuBar> menuBars;

        [RelayCommand]
        void Navigate(MenuBar obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
                return;

            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj.NameSpace);
        }

        [RelayCommand]
        void NavigateHome()
        {
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("IndexView");
        }

        [RelayCommand]
        void LoginOut()
        {
            App.LoginOut(containerProvider);
        }

        partial void OnUserNameChanged(string value)
        {
            if (Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
            {
                Initial = value[0];
                return;
            }
            if (!string.IsNullOrWhiteSpace(value))
            {
                // 获取第一个字符
                char firstChar = value[0];

                // 使用 Pinyin4Net 获取拼音首字母
                string pinyin = Pinyin4Net.GetFirstPinyin(firstChar);
                if (!string.IsNullOrEmpty(pinyin))
                {
                    Initial = char.ToUpper(pinyin[0]); // 转为大写
                }
            }
        }

        void CreateMenuBar()
        {
            MenuBars?.Add(new MenuBar() { Icon = "InformationOutline", Title = "关于本平台", NameSpace = "AboutView" });
            MenuBars?.Add(new MenuBar() { Icon = "TextBoxSearchOutline", Title = "实训记录查看", NameSpace = "RecordView" });
            MenuBars?.Add(new MenuBar() { Icon = "ChartBar", Title = "实训统计与测试", NameSpace = "ChartView" });
            MenuBars?.Add(new MenuBar() { Icon = "AccessPointNetwork", Title = "Modbus 配置", NameSpace = "ModbusView" });
            
        }

        /// <summary>
        /// 配置首页初始化参数
        /// </summary>
        public void Configure()
        {
            UserName = AppSession.UserName;
            CreateMenuBar();
            OnUserNameChanged(UserName);
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("IndexView");
        }
    }
}
