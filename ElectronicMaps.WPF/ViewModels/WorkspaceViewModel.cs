using ElectronicMaps.Application.DTO;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.WPF.Infrastructure.Screens;
using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicMaps.WPF.ViewModels
{
    public class WorkspaceViewModel : BaseScreenViewModel, INavigatedTo
    {
        private readonly IComponentStore _componentStore;

        public ObservableCollection<AnalyzedComponentDto> Components { get; } = new();

        private AnalyzedComponentDto? _selectedComponent;
        public AnalyzedComponentDto? SelectedComponent
        {
            get => _selectedComponent;
            set => SetProperty(ref _selectedComponent, value);
        }


        public WorkspaceViewModel(IComponentStore componentStore) 
        {
            _componentStore = componentStore;
            _componentStore.Changed += OnComponentStoreChanged;
        }

        private void OnComponentStoreChanged(object? sender, StoreChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default)
        {
            var snapshot = _componentStore.GetAll();
            Components.Clear();
            
            foreach (var component in snapshot)
            {
                Components.Add(component);
            }

            SelectedComponent = null;

            return Task.CompletedTask;
        }
    }
}
