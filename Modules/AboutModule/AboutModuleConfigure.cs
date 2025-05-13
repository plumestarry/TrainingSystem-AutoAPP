using AutoAPP.Core.Dialogs;
using AutoAPP.Core.Dialogs.Interface;
using AutoAPP.Core.Service;
using AutoAPP.Core.Service.Interface;
using AutoAPP.Core.ViewModels;
using AutoAPP.Core.Views;
using AboutModule.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AboutModule.ViewModels;

namespace AboutModule
{
    public class AboutModuleConfigure : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AboutView, AboutViewModel>();

        }
    }
}
