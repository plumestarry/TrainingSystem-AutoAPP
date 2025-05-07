using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordModule.Models
{
    public partial class RecordItem : ObservableObject
    {
        [ObservableProperty]
        int index;

        [ObservableProperty]
        string? userName;

        [ObservableProperty]
        string? title;

        [ObservableProperty]
        string? recordTime;

        [ObservableProperty]
        string? content;
        
    }
}
