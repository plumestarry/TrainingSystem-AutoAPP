using AutoAPP.Core.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModbusModule.Methods;
using ModbusModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Prism.Ioc;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.ViewModels
{
    public partial class ModbusContentVM : NavigationViewModel
    {
        public ModbusContentVM(IContainerProvider containerProvider) : base(containerProvider)
        {
            // 初始化集合
            InputItems = new ObservableCollection<ModbusItems>();
            OutputItems = new ObservableCollection<ModbusItems>();
            SelectedInputItem = new ModbusItems(); // 初始化为非 null 值
            SelectedOutputItem = new ModbusItems(); // 初始化为非 null 值
            SelectedCommunicationEntity = CommunicationEntities.FirstOrDefault() ?? string.Empty; // 初始化为非 null 值

            IODataMethod.InitializeDatabase(ConnectionString, CommunicationEntities.Count);
            IODataMethod.LoadDataFromSqlite(ConnectionString, 1, InputItems, OutputItems);
        }

        #region ****************************** Modbus 配置代码 ******************************

        /// <summary>
        /// 输入项集合
        /// </summary>
        [ObservableProperty]
        ObservableCollection<ModbusItems> inputItems;

        /// <summary>
        /// 输出项集合
        /// </summary>
        [ObservableProperty]
        ObservableCollection<ModbusItems> outputItems;

        /// <summary>
        /// Modbus 类型集合
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> modbusTypes = new ObservableCollection<string> { "Bool", "Register" };

        /// <summary>
        /// DataGrid 中当前选中的单个输入项
        /// 需要绑定 DataGrid 的 SelectedItem="{Binding SelectedInputItem}"
        /// </summary>
        [ObservableProperty]
        ModbusItems selectedInputItem;

        [ObservableProperty]
        ModbusItems selectedOutputItem;

        [RelayCommand]
        void AddInputItem()
        {
            AddModbusItem(InputItems);
        }

        [RelayCommand]
        void AddOutputItem()
        {
            AddModbusItem(OutputItems);
        }

        /// <summary>
        /// 删除选中的输入项
        /// </summary>
        [RelayCommand]
        void RemoveInputItem()
        {
            RemoveItem(SelectedInputItem, InputItems);
        }

        /// <summary>
        /// 删除选中的输出项
        /// </summary>
        [RelayCommand]
        void RemoveOutputItem()
        {
            RemoveItem(SelectedOutputItem, OutputItems);
        }

        /// <summary>
        /// 添加实体项
        /// </summary>
        /// <param name="items"></param>
        private void AddModbusItem(ObservableCollection<ModbusItems> items)
        {
            // 将新项添加到集合，ObservableCollection 会自动通知 DataGrid
            items.Add(new ModbusItems
            {
                // 计算新的序号：找到当前集合中的最大序号并加一，如果集合为空，则新序号从1开始
                Index = items.Any() ? items.Max(item => item.Index) + 1 : 1,
                ModbusType = "Bool",
                // 新添加的行，名称和端口为空，待用户填写
                Name = "",
                Port = 0
            });
        }

        private void RemoveItem(ModbusItems SelectedItem, ObservableCollection<ModbusItems> Items)
        {
            // 检查是否选中了单个项 (即 SelectedInputItem 是否不为 null)
            if (SelectedItem == null)
            {
                // 可以选择弹窗提示用户没有选中项
                // MessageBox.Show("请选择一个要删除的项。");
                return;
            }

            // 从 InputItems 集合中移除选中的项
            Items.Remove(SelectedItem);

            // 重新计算并更新剩余项的序号
            // 遍历剩余项并按顺序重新赋值 Index
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Index = i + 1; // 序号从 1 开始
            }
        }

        #endregion ****************************** Modbus 配置代码 ******************************

        #region ****************************** 数据库代码 ******************************

        // SQLite 数据库文件路径
        private string DatabaseFileName = "modbusdata.db";
        private string DatabasePath => Path.Combine(AppContext.BaseDirectory, DatabaseFileName); // 数据库放在应用程序执行目录

        // 数据库连接字符串
        public string ConnectionString => $"Data Source={DatabasePath}";

        /// <summary>
        /// 通信实体集合
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> communicationEntities = new ObservableCollection<string> { "Sort1", "Sort2" };

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadDataCommand))]
        private string selectedCommunicationEntity;

        [RelayCommand]
        void SaveIO()
        {
            UpdateLoading(true);
            var index = CommunicationEntities.IndexOf(SelectedCommunicationEntity);
            IODataMethod.SaveData(ConnectionString, index + 1, InputItems, OutputItems);
            UpdateLoading(false);
        }

        [RelayCommand]
        void LoadData()
        {
        }

        partial void OnSelectedCommunicationEntityChanged(string? oldValue, string newValue)
        {
            UpdateLoading(true);
            if (!string.IsNullOrEmpty(newValue) && !string.IsNullOrEmpty(oldValue))
            {
                var index = CommunicationEntities.IndexOf(oldValue);
                IODataMethod.SaveData(ConnectionString, index + 1, InputItems, OutputItems);
                index = CommunicationEntities.IndexOf(newValue);
                IODataMethod.LoadDataFromSqlite(ConnectionString, index + 1, InputItems, OutputItems);
                LoadDataCommand.Execute(null);
            }
            UpdateLoading(false);
        }

        #endregion ****************************** 数据库代码 ******************************
    }
}
