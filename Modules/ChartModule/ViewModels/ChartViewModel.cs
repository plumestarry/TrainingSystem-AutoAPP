using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.SkiaSharpView;
using System.Collections.Generic;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using LiveChartsCore.Measure;
using System;

namespace ChartModule.ViewModels
{
    // 新的数据类，用于存储每个条形的数据和样式
    public class BarItemData // 不需要继承 ObservableValue unless you need live updates on individual points
    {
        public BarItemData(string name, int value, SolidColorPaint paint)
        {
            Name = name;
            Value = value;
            Paint = paint;
        }

        public string Name { get; set; }
        public int Value { get; set; }
        public SolidColorPaint Paint { get; set; }
    }

    public partial class ChartViewModel : ObservableObject
    {
        // --- Data Sources ---
        // ObservableCollection for dynamic updates of names
        public ObservableCollection<string> ElementNames { get; set; }

        // Dictionary to store dynamic values keyed by name
        public Dictionary<string, int> ElementValues { get; set; }

        // --- Chart Properties (Bindable) ---
        // Use ObservableProperty attribute to automatically generate
        // the property and INotifyPropertyChanged logic
        //[ObservableProperty]
        //private ISeries[] _series;

        [ObservableProperty]
        private ObservableCollection<ISeries> _series;

        [ObservableProperty]
        private Axis[] _xAxes;

        [ObservableProperty]
        private Axis[] _yAxes;

        // --- Simulation Fields ---
        private bool _isUpdating = false;
        private Random _random = new Random();
        // --- Constructor ---
        public ChartViewModel()
        {
            ElementNames = new ObservableCollection<string>();
            ElementValues = new Dictionary<string, int>();
            _series = new ObservableCollection<ISeries>();

            // --- 在这里添加你的示例数据 ---

            // --- 示例数据 A: 两个元素 ---
            // ElementNames.Add("销量");
            // ElementNames.Add("库存");
            // ElementValues["销量"] = 350;
            // ElementValues["库存"] = 120;

            // --- 示例数据 B: 四个元素 ---
            ElementNames.Add("产品 A");
            ElementNames.Add("产品 B");
            ElementNames.Add("产品 C");
            ElementNames.Add("产品 D");
            ElementValues["产品 A"] = 12;
            ElementValues["产品 B"] = 33;
            ElementValues["产品 C"] = 0;
            ElementValues["产品 D"] = 39;

            // --- 示例数据 C: 三个元素 (包含一个缺失值的项) ---
            // ElementNames.Add("任务 1");
            // ElementNames.Add("任务 2");
            // ElementNames.Add("任务 3"); // 这个任务的值不在 ElementValues 中
            // ElementValues["任务 1"] = 95;
            // ElementValues["任务 2"] = 40;
            // // 故意不添加 "任务 3" 的值

            // --- End of Sample Data ---


            // 初始化空的图表属性 (在填充数据之前，确保它们不是 null)
            // 也可以在 UpdateChart() 内部处理首次赋值
            //_series = Array.Empty<ISeries>();
            _series = new ObservableCollection<ISeries>();
            _xAxes = new Axis[] { new Axis() };
            _yAxes = new Axis[] { new Axis() };


            // Optional: Listen to changes in ElementNames if you want the chart to update automatically
            //ElementNames.CollectionChanged += ElementNames_CollectionChanged;

            // 在填充数据后，调用 UpdateChart 来生成图表系列和轴
            UpdateChart();

            _ = StartUpdating();
        }

        // --- Public Method to Update Chart ---
        // --- Public Method to Update Chart ---
        public void UpdateChart()
        {
            // --- Generate Series ---
            var newLabelsList = new List<string>();
            var currentSeriesDict = Series.OfType<ColumnSeries<int>>().ToDictionary(s => s.Name, s => s); // Map existing series by name


            // Define a color palette. You can make this more sophisticated.
            var colors = new SKColor[]
            {
            SKColors.Blue,
            SKColors.Red,
            SKColors.Green,
            SKColors.Orange,
            SKColors.Purple,
            SKColors.Brown,
            SKColors.Cyan,
            SKColors.Magenta,
            SKColors.Lime,
            SKColors.Pink
                // Add more colors if you expect more elements
            };

            // Define label paint (color and font for the numbers on bars)
            var dataLabelPaint = new SolidColorPaint(SKColors.Black); // Black text
                                                                      // Optional: Configure font size, typeface etc.
                                                                      // dataLabelPaint.SKTypeface = SKTypeface.FromFamilyName("Arial");
                                                                      // dataLabelPaint.TextSize = 14;

            var seriesToKeep = new List<ISeries>();
            for (int i = 0; i < ElementNames.Count; i++)
            {
                string elementName = ElementNames[i];

                // Check if the element name exists in the values dictionary
                if (ElementValues.TryGetValue(elementName, out int elementValue))
                {
                    ColumnSeries<int> columnSeries;

                    // **Check if a series with this name already exists**
                    if (currentSeriesDict.TryGetValue(elementName, out columnSeries))
                    {
                        // **Reuse existing series and update its value**
                        // Ensure Values is ObservableCollection<int> with at least one item
                        if (columnSeries.Values is ObservableCollection<int> valuesCollection && valuesCollection.Count > 0)
                        {
                            // **Update the existing value in the collection**
                            // This triggers smooth animation for the bar height
                            valuesCollection[0] = elementValue;
                        }
                        else
                        {
                            // Should not happen if initial creation is correct, but handle as fallback
                            // Recreate the Values collection
                            columnSeries.Values = new ObservableCollection<int> { elementValue };
                        }
                    }
                    // Create a ColumnSeries for each element
                    else
                    {
                        columnSeries = new ColumnSeries<int>
                        {
                            Name = elementName, // Optional, good for tooltips/legends
                                                //Values = new int[] { elementValue }, // Values for this *single* bar
                            Values = new ObservableCollection<int> { elementValue },
                            MaxBarWidth = 100, // **减小 MaxBarWidth，使条形更窄，从而增加相对间距** (可以调整这个值)
                            Padding = 50, // No padding within this single-bar series group
                                          // Assign a unique color by cycling through the palette
                            Fill = new SolidColorPaint(colors[i % colors.Length]),

                            // **启用并配置数值标签**
                            DataLabelsPaint = dataLabelPaint, // 使用定义的标签颜色
                            DataLabelsPosition = DataLabelsPosition.Top, // 标签显示在条形顶部
                            DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}"

                        };
                    }

                    seriesToKeep.Add(columnSeries);
                    newLabelsList.Add(elementName);
                }
                // If a name exists in ElementNames but not ElementValues, it will be skipped.
            }

            // **Update the ViewModel's Series collection using Diffing**
            // This approach modifies the ObservableCollection<ISeries> by only adding/removing
            // items that are different, preserving existing instances.

            // Identify series instances currently in the Series collection
            // but are NOT in the list of series we want to keep (seriesToKeep).
            // These are the series to remove.
            // We need to create a copy of the current Series collection to iterate while modifying it.
            var currentSeriesList = Series.ToList();
            var seriesToRemove = currentSeriesList.Where(s => !seriesToKeep.Contains(s)).ToList();

            // Identify series instances that are in the list we want to keep (seriesToKeep)
            // but are NOT currently in the Series collection.
            // These are the series to add.
            var seriesToAdd = seriesToKeep.Where(s => !currentSeriesList.Contains(s)).ToList();

            // **Perform the removals from the ViewModel's Series collection**
            foreach (var series in seriesToRemove)
            {
                Series.Remove(series);
            }

            // **Perform the additions to the ViewModel's Series collection**
            // Note: The order of addition might affect the visual order on the chart
            // for categorical axes if not carefully managed. For simple cases, adding at the end is fine.
            foreach (var series in seriesToAdd)
            {
                Series.Add(series);
            }


            // --- Configure X-Axis ---
            XAxes = new Axis[]
            {
            new Axis
            {
                Labels = newLabelsList.ToArray(),
                LabelsRotation = 0, // Keep labels horizontal
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)), // Light gray separators
                SeparatorsAtCenter = false,
                TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)), // Dark gray ticks
                TicksAtCenter = true,
                // Ensure all labels are shown if possible
                ForceStepToMin = true,
                MinStep = 1
            }
            };

            // --- Configure Y-Axis ---
            // Y-axis typically scales automatically, but setting MinLimit=0 is good for bar charts
            // Setting MaxLimit is optional if you always want the 0-500 range, otherwise let it auto-scale
            YAxes = new Axis[]
            {
            new Axis
            {
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                MinLimit = 0, // Start the axis at 0
                //MaxLimit = 500 // Optional: Uncomment if you want a fixed max limit

            }
            };
        }

        // --- Handle ObservableCollection Changes (Optional) ---
        //private void ElementNames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    // When ElementNames changes, update the chart
        //    // This is triggered if items are added/removed from ElementNames.
        //    // If you only change values in ElementValues, you must call UpdateChart() manually.
        //    UpdateChart();
        //}

        // Public method to start the update loop
        public async Task StartUpdating()
        {
            if (_isUpdating) return;
            _isUpdating = true;
            Console.WriteLine("Starting data update simulation...");

            while (_isUpdating)
            {
                await Task.Delay(2000);

                // **更新 ElementValues 中的数据**
                foreach (string name in ElementNames) // Iterate names to ensure we only update relevant values
                {
                    if (ElementValues.ContainsKey(name))
                    {
                        // Randomly change value (e.g., add/subtract)
                        // To make values fluctuate more, you could add/subtract a random amount
                        // Or just set a new random value within a range
                        int change = _random.Next(0, 51); // Change between -50 and +50
                        ElementValues[name] += change;

                        // Optional: Keep values within a range if needed, though Y-axis is fixed 0-500
                        // ElementValues[name] = Math.Max(0, ElementValues[name]); // Ensure non-negative
                        // ElementValues[name] = Math.Min(600, ElementValues[name]); // Optional upper bound for data

                    }
                }

                // **Call UpdateChart method to update the series values and notify UI**
                // Because UpdateChart now updates the ObservableCollection<int> inside each series,
                // LiveCharts detects the change and animates.
                // If ElementNames changes, UpdateChart also handles adding/removing series instances.
                UpdateChart(); // Update chart config (labels, series collection)

                Console.WriteLine($"Data updated at {DateTime.Now.ToShortTimeString()}.");
            }

            Console.WriteLine("Data update simulation stopped.");
        }


    }
}