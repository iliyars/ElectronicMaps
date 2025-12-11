using ElectronicMaps.Application.DTO;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.WPF.Infrastructure.Screens;
using ElectronicMaps.WPF.ViewModels.Components;
using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace ElectronicMaps.WPF.ViewModels
{
    public class WorkspaceViewModel : BaseScreenViewModel, INavigatedTo
    {
        private readonly IComponentStore _componentStore;

        public ObservableCollection<AnalyzedComponentDto> Components { get; } = new();
        public ObservableCollection<AnalyzedComponentDto> FiltredComponents { get; } = new();
        public ObservableCollection<FormFilterItem> FormFilters { get; } = new();

        private FormFilterItem? _selectedForm;
        public FormFilterItem? SelectedForm
        {
            get => _selectedForm;
            set 
            {
                if (SetProperty(ref _selectedForm, value))
                {
                    ApplyForm();
                }

            }
        }

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
            GetAllItemsFromStore();

            InitFormFilters();

            return Task.CompletedTask;
        }
        /// <summary>
        /// Получает все компоненты из хранилища.
        /// </summary>
        private void GetAllItemsFromStore()
        {
            var snapshot = _componentStore.GetAll();
            Components.Clear();

            foreach (var component in snapshot)
            {
                Components.Add(component);
            }

            SelectedComponent = null;
        }

        /// <summary>
        /// Заполняем список испульзуемы форм для компонентов для checkbox'a
        /// </summary>
        private void InitFormFilters()
        {

            FormFilters.Clear();

            var familyFormCode = "FORM_4";
            FormFilters.Add(new FormFilterItem(familyFormCode, "Форма 4"));

            var componentFormItems = Components
                .Where(c => !string.IsNullOrWhiteSpace(c.ComponentFormTypeCode))
                .GroupBy(
                    c => c.ComponentFormTypeCode!.Trim(),                  // ключ = код формы
                    StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    
                    var displayName = g.Select(x => x.ComponentFormDisplayName)
                        .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));

                    return new FormFilterItem(code: g.Key,displayName: displayName ?? g.Key);
                })
                .OrderBy(item => item.Code)
                .ToList();

            foreach(var item in componentFormItems)
            {
                if (!item.Code.Equals("FORM_4", StringComparison.OrdinalIgnoreCase))
                    FormFilters.Add(item);
            }

        }

        private void ApplyForm()
        {
            const string familyFormCode = "FORM_4";

            FiltredComponents.Clear();

            string selectedCode = SelectedForm.Code;

            if (selectedCode.Equals(familyFormCode, StringComparison.OrdinalIgnoreCase))
            {
                var families = Components
                    .Where(c => c.FamilyFormTypeCode == familyFormCode)
                    .ToList();

                foreach (var family in families)
                    FiltredComponents.Add(family);

                return;
            }

           var components = Components
                .Where(c => c.ComponentFormTypeCode == selectedCode)
                .ToList();

            foreach (var component in components)
            {
                FiltredComponents.Add(component);
            }
        }
    }
}
