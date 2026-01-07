using ElectronicMaps.WPF.Features.Workspace.Components.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Services.Factories
{
    /// <summary>
    /// Фабрика для создания ViewModels через DI
    /// </summary>
    public interface IViewModelFactory
    {
            /// <summary>
            /// Создать ViewModel для создания компонента
            /// </summary>
            CreateComponentViewModel CreateCreateComponentViewModel();
    }
}
