using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAPP.Core.Dialogs.Interface;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoAPP.Core.ViewModels
{
    public partial class MsgViewModel : ObservableObject, IDialogHostAware
    {
        public MsgViewModel()
        {
        }

        [ObservableProperty]
        string? title;

        [ObservableProperty]
        string? content;

        public string DialogHostName { get; set; } = "Root";

        [RelayCommand]
        public void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult { Result = ButtonResult.No });
        }

        [RelayCommand]
        public void Save()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters param = new DialogParameters();
                DialogHost.Close(DialogHostName, new DialogResult { Result = ButtonResult.OK, Parameters = param });
            }
        }

        public void OnDialogOpend(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Title"))
                Title = parameters.GetValue<string>("Title");

            if (parameters.ContainsKey("Content"))
                Content = parameters.GetValue<string>("Content");
        }
    }
}
