using ElectronicMaps.WPF.Features.Workspace.Components.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Services.Factories
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Создать ViewModel для создания компонента
        /// </summary>
        public CreateComponentViewModel CreateCreateComponentViewModel()
        {
            return _serviceProvider.GetRequiredService<CreateComponentViewModel>();
        }
    }
}
