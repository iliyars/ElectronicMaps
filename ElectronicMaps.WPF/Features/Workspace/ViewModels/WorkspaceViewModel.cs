using ElectronicMaps.Application.Stores;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.GridRows;
using ElectronicMaps.WPF.Infrastructure.Screens;
using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Features.Workspace.ViewModels
{
    public class WorkspaceViewModel : BaseScreenViewModel, INavigatedTo
    {
        private readonly IComponentStore _componentStore;

        private bool _isDetailsOpen;
        public bool IsDetailsOpen
        {
            get => _isDetailsOpen;
            set => SetProperty(ref _isDetailsOpen, value);
        }


        /// <summary>
        /// Все компоненты из xml.
        /// </summary>
        public ObservableCollection<ImportedRowViewModel> ImportedComponents { get; } = new();

        public Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
