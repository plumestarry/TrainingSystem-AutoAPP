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
using AutoAPP.Views;
using AutoAPP.ViewModels;
using AboutModule.Views;
using AboutModule;
using Prism.Ioc;
using ModbusModule;
using ChartModule;
using ModbusModule.ViewModels;
using ModbusModule.Views;
using RecordModule;

namespace AutoAPP;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return null;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<IndexView>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<CoreModuleConfigure>();
        moduleCatalog.AddModule<LoginModuleConfigure>();
        moduleCatalog.AddModule<AboutModuleConfigure>();
        moduleCatalog.AddModule<ModbusModuleConfigure>();
        moduleCatalog.AddModule<ChartModuleConfigure>();
        moduleCatalog.AddModule<RecordModuleConfigure>();
    }

    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);
    }

    public static void LoginOut(IContainerProvider containerProvider)
    {
        Current.MainWindow.Hide();

        var dialog = containerProvider.Resolve<IDialogService>();

        dialog.ShowDialog("LoginView", callback =>
        {
            if (callback.Result != ButtonResult.OK)
            {
                Current.Shutdown();
                return;
            }

            Current.MainWindow.Show();
        });
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // 在这里解析 MainView，设置为应用程序的主窗口
        Current.MainWindow = Container.Resolve<MainView>();

        var dialog = Container.Resolve<IDialogService>();

        dialog.ShowDialog("LoginView", callback =>
        {
            if (callback.Result != ButtonResult.OK)
            {
                Current.Shutdown();
                return;
            }

            Current.MainWindow.Show();

            // 解决导航异常问题
            RegionManager.SetRegionManager(Current.MainWindow, ContainerLocator.Container.Resolve<IRegionManager>());
            RegionManager.UpdateRegions();

            var service = Current.MainWindow.DataContext as IConfigureService;
            service?.Configure();


        });
    }

}

