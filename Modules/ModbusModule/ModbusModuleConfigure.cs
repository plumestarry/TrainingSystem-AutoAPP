﻿using AutoAPP.Core.Dialogs;
using AutoAPP.Core.Dialogs.Interface;
using AutoAPP.Core.Service;
using AutoAPP.Core.Service.Interface;
using AutoAPP.Core.ViewModels;
using AutoAPP.Core.Views;
using ModbusModule.Methods;
using ModbusModule.Methods.Interface;
using ModbusModule.ViewModels;
using ModbusModule.ViewModels.Dialogs;
using ModbusModule.Views;
using ModbusModule.Views.Dialogs;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule
{
    public class ModbusModuleConfigure : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterForNavigation<ModbusView, ModbusViewModel>();
            containerRegistry.Register<IModbusTcp, ModbusTcp>();

            containerRegistry.RegisterForNavigation<ModbusView, ModbusViewModel>();
            containerRegistry.RegisterForNavigation<ConnectView, ConnectViewModel>();
        }
    }
}
