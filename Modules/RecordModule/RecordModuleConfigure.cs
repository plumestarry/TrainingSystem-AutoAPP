using AutoAPP.Core.Dialogs;
using AutoAPP.Core.Dialogs.Interface;
using AutoAPP.Core.Service;
using AutoAPP.Core.Service.Interface;
using AutoAPP.Core.ViewModels;
using AutoAPP.Core.Views;
using Prism.Ioc;
using Prism.Modularity;
using RecordModule.ViewModels;
using RecordModule.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordModule
{
    public class RecordModuleConfigure : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<RecordView, RecordViewModel>();
        }
    }
}
