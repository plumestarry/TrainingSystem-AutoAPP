using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Models
{

    public partial class ModbusConfig : ObservableObject
    {
        [ObservableProperty]
        string? iPAddress;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        byte slaveID;
    }
}
