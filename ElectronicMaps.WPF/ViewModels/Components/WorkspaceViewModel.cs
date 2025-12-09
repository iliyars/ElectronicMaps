using ElectronicMaps.Application.Stores;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.WPF.Infrastructure.Screens;
using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicMaps.WPF.ViewModels.Components
{
    public class WorkspaceViewModel : BaseScreenViewModel
    {
        private readonly IComponentStore _componentStore;

        public WorkspaceViewModel()
        {
                
        }

        public WorkspaceViewModel(IComponentStore componentStore) 
        {
            _componentStore = componentStore;
        }

      
    }
}
