using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System.Windows.Threading;
using System.Xml.Linq;
using Path = System.IO.Path;
using System.Reflection.Metadata;
using System.Windows.Resources;
using IndexModule.Models;

namespace IndexModule.ViewModels
{
    public partial class IndexViewModel : NavigationViewModel
    {
        private readonly IRecordService recordService;

        public IndexViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            Title = $"你好，{AppSession.UserName}  欢迎使用分拣装置实训平台";
            InfoCards = new ObservableCollection<InfoCard>();

            // 初始化并启动定时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            timer.Tick += Timer_Tick; // 绑定 Tick 事件处理程序
            timer.Start();

            // 初始化 Now 属性为当前时间
            Now = DateTime.Now;

            this.recordService = containerProvider.Resolve<IRecordService>();
            LoadInfoCardsFromResource("../Models/Content.xml");
        }

        [ObservableProperty]
        private DateTime now;

        private DispatcherTimer timer;

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Now = DateTime.Now; // 更新 _now 字段 (通过自动生成的 Now 属性的 set 访问器)
        }

        [ObservableProperty]
        private SummaryDto summary;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string completedRatio = "0%";

        [ObservableProperty]
        private ISeries[] series;

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (AppSession.UserName == "Guest")
                return;
            try
            {
                UpdateLoading(true);
                var summaryResult = await recordService.SummaryAsync(AppSession.UserName);
                if (summaryResult.Status)
                {
                    Summary = summaryResult.Result;
                    Refresh();
                }
                base.OnNavigatedTo(navigationContext);
            }
            finally
            {
                UpdateLoading(false);
            }
        }

        private void Refresh()
        {
            CompletedRatio = Summary.CompletedRatio;
            Series = [
                new PieSeries<int>
                {
                    Name = "IsPassed",
                    Values = [Summary.CompletedCount],
                    Stroke = null,
                    Fill = new RadialGradientPaint(new SKColor(255, 160, 0), new SKColor(255, 153, 0)),
                    Pushout = 10,
                    OuterRadiusOffset = 20
                },
                new PieSeries<int>
                {
                    Name = "NotPassed",
                    Values = [Summary.Sum - Summary.CompletedCount],
                    Stroke = null,
                    Fill = new RadialGradientPaint(new SKColor(255, 255, 255), new SKColor(255, 255, 255))
                }
            ];
        }

        [ObservableProperty]
        private ObservableCollection<InfoCard> infoCards;

        // >>> 修改: 从嵌入的资源加载和解析 InfoCard 数据的方法 <<<
        private void LoadInfoCardsFromResource(string resourceName)
        {
            InfoCards.Clear();

            try
            {
                // 构建 WPF Resource 的 Pack URI
                // pack://application:,,,/ 表示应用程序的入口程序集根目录
                // resourceName 就是你在项目中文件的相对路径，例如 "InfoCards.xml" 或 "Data/InfoCards.xml"
                string uriString = $"pack://application:,,/{resourceName}";
                Uri resourceUri = new Uri(uriString, UriKind.Absolute);

                // 使用 Application.GetResourceStream 获取资源流信息
                StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

                // 检查资源是否存在
                if (streamInfo != null)
                {
                    // 从 StreamResourceInfo 中获取 Stream
                    using (Stream stream = streamInfo.Stream) // 使用 using 确保流在使用后被正确关闭和释放
                    {
                        // 使用 LINQ to XML 从 Stream 中加载并解析 XML
                        XDocument doc = XDocument.Load(stream);

                        // 查找所有的 InfoCard 元素并遍历 (解析逻辑与之前相同)
                        if (doc.Root != null && doc.Root.Name == "InfoCards")
                        {
                            foreach (XElement infoCardElement in doc.Root.Elements("InfoCard"))
                            {
                                InfoCard card = new InfoCard
                                {
                                    Header = infoCardElement.Element("Header")?.Value,
                                    Content = infoCardElement.Element("Content")?.Value
                                };
                                InfoCards.Add(card);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"资源 '{resourceName}' 格式不正确：根元素不是 <InfoCards>。");
                        }
                    } // using 块结束时，stream 会被自动释放
                }
                else
                {
                    // 处理资源未找到的情况
                    System.Diagnostics.Debug.WriteLine($"资源 '{resourceName}' 未找到。请确认 Build Action 设置为 Resource。");
                }
            }
            catch (Exception ex)
            {
                // 处理加载或解析资源时发生的其他异常
                System.Diagnostics.Debug.WriteLine($"加载或解析资源 '{resourceName}' 时发生错误：{ex.Message}");
            }
        }
    }
}