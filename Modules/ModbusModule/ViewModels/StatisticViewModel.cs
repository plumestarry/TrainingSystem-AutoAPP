using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ModbusModule.Methods;
using ModbusModule.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace ModbusModule.ViewModels
{
    public partial class StatisticViewModel : ObservableObject
    {
        public StatisticViewModel(ObservableCollection<ModbusItems> OutputItems)
        {
            Colors = _colors;

            // 在构造函数中获取 Dispatcher
            _dispatcher = Dispatcher.CurrentDispatcher; // 或者 DispatcherQueue.GetForCurrentThread();

            // 初始化数据集合
            ChartData = OutputItems;
            Series = new ObservableCollection<ISeries>();

            // 订阅 ChartData 的集合变化事件，用于处理项目的添加和移除
            ChartData.CollectionChanged += ChartData_CollectionChanged;

            // 初始化模拟数据
            InitializeChartData();

            // 根据初始数据创建图表 Series 和 Axes
            SynchronizeChartSeriesAndAxes();
        }

        // 使用 ObservableCollection<ChartItem> 作为主要数据源
        [ObservableProperty]
        private ObservableCollection<ModbusItems> chartData;

        // Chart control binds to these properties
        [ObservableProperty]
        private ObservableCollection<ISeries> series;

        [ObservableProperty]
        private Axis[]? xAxes;

        [ObservableProperty]
        private Axis[]? yAxes;

        // 使用 Dispatcher 来确保 UI 更新在主线程进行
        private Dispatcher _dispatcher;

        // 定义一组颜色，可以更灵活地管理
        private readonly SKColor[] _colors = new SKColor[]
        {
            SKColors.SpringGreen,
            SKColors.SkyBlue,
            SKColors.SlateGray,
            SKColors.Black
        };

        public SKColor[] Colors { get; }

        /// <summary>
        /// Initializes the initial chart data.
        /// </summary>
        private void InitializeChartData()
        {
            // 订阅 ModbusItems 中 ModbusType 为 Register 的项的属性变化
            foreach (var item in ChartData)
            {
                // 假设 item 对象有一个 ModbusType 属性，并且有一个枚举类型或常量表示 Register
                // 请根据你的实际 ModbusItem 类结构调整这里的属性访问和比较
                if (item.ModbusType == "Register") // 假设 ModbusType 是一个枚举，且 Register 是其中的一个值
                {
                    item.PropertyChanged += ChartItem_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Gets a color from the predefined palette based on index.
        /// </summary>
        /// <param name="index">The index to get the color for.</param>
        /// <returns>An SKColor.</returns>
        private SKColor GetColor(int index)
        {
            return _colors[index % _colors.Length];
        }

        /// <summary>
        /// Synchronizes the ISeries collection and Axes based on the current ChartData.
        /// This method handles adding/removing series and updates axis labels.
        /// Value updates are handled by ChartItem_PropertyChanged.
        /// </summary>
        public void SynchronizeChartSeriesAndAxes()
        {
            // Fix for CS8714 and CS8621: Ensure that the key selector function for ToDictionary does not return null.
            // Use null-coalescing operator to provide a default value for `Name` if it is null.

            var currentSeriesDict = Series
                .OfType<ColumnSeries<int>>()
                .ToDictionary(s => s.Name ?? string.Empty, s => s);
            var seriesToKeep = new HashSet<ISeries>();
            var newLabels = new List<string>() { "Product" };
            var count = -1;

            // Iterate through the desired data (ChartData)
            for (int i = 0; i < ChartData.Count; i++)
            {
                if (ChartData[i].ModbusType != "Register") // 假设 ModbusType 是一个枚举，且 Register 是其中的一个值
                {
                    continue;
                }

                var chartItem = ChartData[i];
                ColumnSeries<int>? columnSeries;
                count++;

                // Try to find an existing series for this item
                if (currentSeriesDict.TryGetValue(chartItem.Name, out columnSeries))
                {
                    // Found existing series, ensure its properties match ChartItem (except Value, handled by property change)
                    // Note: Value is updated by ChartItem_PropertyChanged, no need to update it here
                    // Only update properties that are static for the series like Fill if they could change (less common)
                    if (!(columnSeries.Fill is SolidColorPaint solidColorPaint) || solidColorPaint.Color != _colors[count])
                    {
                        // Optional: Update color if it can change per item
                        // columnSeries.Fill = new SolidColorPaint(chartItem.Color);
                    }

                    seriesToKeep.Add(columnSeries); // Mark this series to keep
                }
                else
                {
                    // No existing series, create a new one
                    columnSeries = new ColumnSeries<int>
                    {
                        Name = chartItem.Name,
                        Values = new ObservableCollection<int> { chartItem.Status }, // Initialize with current value
                        MaxBarWidth = 96, // Example properties
                        Padding = 48,
                        Fill = new SolidColorPaint(_colors[count]), // Use color from ChartItem
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = DataLabelsPosition.Top,
                        DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}"
                    };
                    Series.Add(columnSeries); // Add the new series to the collection
                    seriesToKeep.Add(columnSeries); // Mark this new series to keep
                }
            }

            // Identify and remove series that no longer exist in ChartData
            var seriesToRemove = Series.Where(s => !seriesToKeep.Contains(s)).ToList();
            foreach (var series in seriesToRemove)
            {
                Series.Remove(series);
                // If the removed series was a ColumnSeries<int>, we might need to find the corresponding ChartItem
                // and unsubscribe from its PropertyChanged event if it wasn't already handled by ChartData_CollectionChanged.
                // However, the CollectionChanged handler should manage subscriptions correctly.
            }

            // Update Axes
            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = newLabels.ToArray(),
                    LabelsRotation = 0,
                    //SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    SeparatorsPaint = null,
                    SeparatorsAtCenter = false,
                    //TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
                    TicksPaint = null,
                    TicksAtCenter = true,
                    ForceStepToMin = true,
                    MinStep = 1
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    //SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    MinLimit = 0,
                    // MaxLimit might be needed for better scaling if values grow large
                    // MaxLimit = ChartData.Any() ? ChartData.Max(item => item.Value) * 1.2 : 10
                }
            };
        }

        /// <summary>
        /// Handles changes in the ChartData collection (items added or removed).
        /// Updates series and axes accordingly.
        /// </summary>
        private void ChartData_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Ensure updates happen on the UI thread
            _dispatcher.Invoke(() =>
            {
                if (e.NewItems != null)
                {
                    foreach (ModbusItems newItem in e.NewItems)
                    {
                        if (newItem.ModbusType == "Register") // 假设 ModbusType 是一个枚举，且 Register 是其中的一个值
                        {
                            newItem.PropertyChanged += ChartItem_PropertyChanged;
                        }
                        // Subscribe to property changes of new items
                        // Add corresponding series - handled by SynchronizeChartSeriesAndAxes,
                        // but we call it after collection changes.
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (ModbusItems oldItem in e.OldItems)
                    {
                        if (oldItem.ModbusType == "Register") // 假设 ModbusType 是一个枚举，且 Register 是其中的一个值
                        {
                            oldItem.PropertyChanged += ChartItem_PropertyChanged;
                        }
                        // Unsubscribe from property changes of old items to prevent memory leaks
                        // Remove corresponding series - handled by SynchronizeChartSeriesAndAxes
                    }
                }

                // Re-synchronize the entire chart series and axes after collection changes
                SynchronizeChartSeriesAndAxes();
            });
        }

        /// <summary>
        /// Handles property changes on individual ChartItem objects.
        /// This is triggered when a ChartItem's Value changes.
        /// </summary>
        private void ChartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Ensure updates happen on the UI thread
            _dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(ModbusItems.Status) && sender is ModbusItems changedItem)
                {
                    // Find the corresponding ColumnSeries by name
                    var columnSeries = Series.OfType<ColumnSeries<int>>()
                                             .FirstOrDefault(s => s.Name == changedItem.Name);

                    if (columnSeries != null && columnSeries.Values is ObservableCollection<int> valuesCollection && valuesCollection.Count > 0)
                    {
                        // Update the value in the Series' ObservableCollection
                        // This triggers LiveCharts2 to animate the bar height
                        valuesCollection[0] = changedItem.Status;
                        // Console.WriteLine($"Updated {changedItem.Name} value to {changedItem.Value}"); // Optional: Log updates
                    }
                    else if (columnSeries != null)
                    {
                        // This case shouldn't ideally happen if series are created correctly,
                        // but as a fallback, re-assign the values collection.
                        columnSeries.Values = new ObservableCollection<int> { changedItem.Status };
                    }
                    // If columnSeries is null, it means the item exists in ChartData but not in Series,
                    // which indicates a potential synchronization issue or a temporary state.
                    // The SynchronizeChartSeriesAndAxes method should eventually fix this.
                }
                // If other properties like Name or Color could change on an existing item,
                // you would handle them here and potentially trigger a full re-synchronization
                // or update series properties directly if supported by LiveCharts.
                // For simplicity, we assume Name and Color are static after creation.
            });
        }

        // Add a Dispose method to clean up subscriptions
        public void Dispose()
        {
            if (ChartData != null)
            {
                // Unsubscribe from CollectionChanged
                ChartData.CollectionChanged -= ChartData_CollectionChanged;

                // Unsubscribe from PropertyChanged for all items
                foreach (var item in ChartData)
                {
                    if (item.ModbusType == "Register") // 假设 ModbusType 是一个枚举，且 Register 是其中的一个值
                    {
                        item.PropertyChanged -= ChartItem_PropertyChanged;
                    }
                }
            }

            // Clear series and axes if necessary
            Series?.Clear();
            XAxes = null;
            YAxes = null;

            // Dispose of other resources if applicable
        }

        // In a real application, you might override methods like OnNavigatedFrom
        // from your NavigationViewModel base class to call Dispose.
    }
}