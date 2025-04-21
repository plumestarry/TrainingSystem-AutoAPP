using AutoAPP.Core.Events;
using System.Configuration;
using System.Data;
using System.Windows;
using LoginModule;
using AutoAPP.Core.Dialogs.Interface;
using AutoAPP.Core;
using AutoAPP.Core.Dialogs;
using AutoAPP.Core.ViewModels;
using AutoAPP.Core.Views;
using AutoAPP.Core.Service.Client;

namespace AutoAPP;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<CoreModuleConfigure>();
        moduleCatalog.AddModule<LoginModuleConfigure>();
    }

    public static void LoginOut(IContainerProvider containerProvider)
    {
        Current.MainWindow.Hide();

        var dialog = containerProvider.Resolve<IDialogService>();

        dialog.ShowDialog("LoginView", callback =>
        {
            if (callback.Result != ButtonResult.OK)
            {
                Application.Current.Shutdown();
                //Environment.Exit(0);
                return;
            }

            Current.MainWindow.Show();
        });
    }

    protected override void OnInitialized()
    {
        var dialog = Container.Resolve<IDialogService>();

        dialog.ShowDialog("LoginView", callback =>
        {
            if (callback.Result != ButtonResult.OK)
            {
                Application.Current.Shutdown();
                //Environment.Exit(0);
                return;
            }

            var service = App.Current.MainWindow.DataContext as IConfigureService;
            service?.Configure();
            base.OnInitialized();
        });
    }

}

