using CommunityToolkit.Mvvm.Messaging.Messages;
using ModbusModule.Methods.Interface;
using ModbusModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.ViewModels
{
    public class CreatedMessage(StatisticViewModel statisticViewModel, IModbusTcp ModbusService) : ValueChangedMessage<StatisticViewModel>(statisticViewModel)
    {
        public IModbusTcp ModbusService { get; } = ModbusService;
    }
}
