using AutoAPP.Core.Extensions;
using AutoAPP.Core.Service.Interface;
using AutoAPP.Core.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecordModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordModule.ViewModels
{
    public partial class RecordViewModel : NavigationViewModel
    {
        private readonly ILoginService loginService;
        private readonly IRecordService recordService;

        public RecordViewModel(ILoginService loginService, IRecordService recordService, IContainerProvider containerProvider) : base(containerProvider)
        {
            this.loginService = loginService;
            this.recordService = recordService;
            NotPassItems = new ObservableCollection<RecordItem>();
            PassItems = new ObservableCollection<RecordItem>();
            _ = GetRecord();
        }

        # region ****************************** 记录筛选代码 ******************************

        [ObservableProperty]
        ObservableCollection<string> communicationEntities = new ObservableCollection<string> { "All", "Sort1", "Sort2" };

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadDataCommand))]
        [NotifyPropertyChangedRecipients]
        private string selectedCommunicationEntity = "All";

        [RelayCommand]
        void LoadData()
        {
        }

        partial void OnSelectedCommunicationEntityChanged(string? oldValue, string newValue)
        {
            UpdateLoading(true);
            if (!string.IsNullOrEmpty(newValue) && !string.IsNullOrEmpty(oldValue))
            {
                GetFilterRecord(newValue);
                LoadDataCommand.Execute(null);
            }
            UpdateLoading(false);
        }

        # endregion ****************************** 记录筛选代码 ******************************

        # region ****************************** 服务代码 ******************************

        async Task GetRecord()
        {
            if (AppSession.UserName == "Guest")
            {
                return;
            }
            UpdateLoading(true);
            try
            {
                var result = await recordService.GetAllAsync(AppSession.UserName);
                if (result.Status)
                {
                    var records = result.Result;
                    foreach (var record in records)
                    {
                        if (record.Content == "3")
                        {
                            PassItemsAll.Add(new RecordItem
                            {
                                Index = PassItemsAll.Any() ? PassItemsAll.Max(item => item.Index) + 1 : 1,
                                UserName = record.UserName,
                                Title = record.Title,
                                Content = record.Content,
                                RecordTime = $"{record.CreateDate:G}",
                            });

                            PassItems.Add(new RecordItem
                            {
                                Index = PassItems.Any() ? PassItems.Max(item => item.Index) + 1 : 1,
                                UserName = record.UserName,
                                Title = record.Title,
                                Content = record.Content,
                                RecordTime = $"{record.CreateDate:G}",
                            });
                        }
                        else
                        {
                            NotPassItemsAll.Add(new RecordItem
                            {
                                Index = NotPassItemsAll.Any() ? NotPassItemsAll.Max(item => item.Index) + 1 : 1,
                                UserName = record.UserName,
                                Title = record.Title,
                                Content = record.Content,
                                RecordTime = $"{record.CreateDate:G}",
                            });

                            NotPassItems.Add(new RecordItem
                            {
                                Index = NotPassItems.Any() ? NotPassItems.Max(item => item.Index) + 1 : 1,
                                UserName = record.UserName,
                                Title = record.Title,
                                Content = record.Content,
                                RecordTime = $"{record.CreateDate:G}",
                            });
                        }
                    }
                }

                if (AppSession.UserName == "Admin")
                {
                    var login = await loginService.GetAllAsync();
                    CommunicationEntities.Clear();
                    CommunicationEntities.Add("All");
                    foreach (var item in login.Result)
                    {
                        CommunicationEntities.Add(item);
                    }
                    CommunicationEntities.Remove("Admin");
                }
            }
            catch (Exception)
            {
                aggregator.SendMessage("获取实训记录失败！");
            }
            finally
            {
                UpdateLoading(false);
            }
        }

        # endregion ****************************** 服务代码 ******************************

        # region ****************************** 实训记录代码 ******************************

        [ObservableProperty]
        ObservableCollection<RecordItem> notPassItems;

        [ObservableProperty]
        ObservableCollection<RecordItem> passItems;

        ObservableCollection<RecordItem> NotPassItemsAll { get; set; } = new ObservableCollection<RecordItem>();

        ObservableCollection<RecordItem> PassItemsAll { get; set; } = new ObservableCollection<RecordItem>();

        void GetFilterRecord(string newValue)
        {
            if (SelectedCommunicationEntity == "All")
            {
                NotPassItems = NotPassItemsAll;
                PassItems = PassItemsAll;
                ReIndexItems(NotPassItems);
                ReIndexItems(PassItems);
            }
            else if (AppSession.UserName == "Admin")
            {
                NotPassItems = [.. NotPassItemsAll.Where(item => item.UserName == newValue)];
                PassItems = [.. PassItemsAll.Where(item => item.UserName == newValue)];
                ReIndexItems(NotPassItems);
                ReIndexItems(PassItems);
            }
            else
            {
                NotPassItems = [.. NotPassItemsAll.Where(item => item.Title == newValue)];
                PassItems = [.. PassItemsAll.Where(item => item.Title == newValue)];
                ReIndexItems(NotPassItems);
                ReIndexItems(PassItems);
            }
        }

        public static void ReIndexItems(ObservableCollection<RecordItem> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].Index = i + 1;
            }
        }

        # endregion ****************************** 实训记录代码 ******************************
    }
}
