using AutoAPP.Core.Dialogs;
using AutoAPP.Core.Dialogs.Interface;
using AutoAPP.Core.Service;
using AutoAPP.Core.Service.Interface;
using AutoAPP.Core.ViewModels;
using AutoAPP.Core.Views;
using LoginModule.ViewModels;
using LoginModule.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginModule
{
    public class LoginModuleConfigure : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ILoginService, LoginService>();

            containerRegistry.RegisterDialog<LoginView, LoginViewModel>();

        }
    }
}
