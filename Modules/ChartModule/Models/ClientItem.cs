using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChartModule.Models
{
    public partial class ClientItem : ObservableObject
    {
        [ObservableProperty]
        string? itemColor;

        [ObservableProperty]
        string? itemLabel;

        [ObservableProperty]
        int itemValue;
    }
}
