using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ModbusModule.Models
{
    public partial class ModbusItems : ObservableObject
    {
        [ObservableProperty]
        int index;

        [ObservableProperty]
        string? modbusType;

        [ObservableProperty]
        string? name;

        [ObservableProperty]
        int port = 0;
    }
}
