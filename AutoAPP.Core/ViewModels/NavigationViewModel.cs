using AutoAPP.Core.Events;
using AutoAPP.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAPP.Core.ViewModels
{
    public class NavigationViewModel(IContainerProvider containerProvider) : ObservableRecipient, INavigationAware
    {
        private readonly IContainerProvider containerProvider = containerProvider;
        public readonly IEventAggregator aggregator = containerProvider.Resolve<IEventAggregator>();

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public void UpdateLoading(bool IsOpen)
        {
            aggregator.UpdateLoading(new UpdateModel()
            {
                IsOpen = IsOpen
            });
        }
    }
}
