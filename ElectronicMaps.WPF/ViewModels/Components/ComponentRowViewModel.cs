using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.ViewModels.Components
{
    public class ComponentRowViewModel : ObservableObject
    {
        public ComponentRowViewModel(ComponentFormResultDto component, int originalIndex, int occurrenceIndex = 1, int occurrenceTotal = 1)
        {
            Component = component;
            OriginalIndex = originalIndex;
            OccurrencesIndex = occurrenceIndex;
            OccurrencesTotal = occurrenceTotal;
        }

        public ComponentFormResultDto Component { get; set; }
        public int OriginalIndex { get; set; }
        public int OccurrencesIndex { get; set; }
        public int OccurrencesTotal { get; set; }

        public string ComponentName => Component.ComponentName;
        private bool _found;
        public bool Found
        {
            get => Component.Found;
            set => SetProperty(ref _found, value);
        }
        public int ParametersCount => Component.Parameters?.Count ?? 0;


        private bool _isSelected = false;
        public bool IsSelected 
        { 
            get => _isSelected;
            set 
            {
                SetProperty(ref _isSelected, value);
                OnSelectionChanged?.Invoke(this, EventArgs.Empty);
            } 
        }

        private string _formStatus = "none";
        public string FormStatus
        {
            get => _formStatus;
            set => SetProperty(ref _formStatus, value);
        }

        public IRelayCommand ShowDetailsCommand { get; }
        public IRelayCommand AddToDbCommand { get; }

        public Action<ComponentRowViewModel>? OnShowDetails {  get; set; }
        public Action<ComponentRowViewModel>? OnAddToDb { get; set; }
        public EventHandler? OnSelectionChanged { get; set; }

        public string DisplayName => OccurrencesTotal > 1 ? $"{ComponentName} ({OccurrencesIndex}/{OccurrencesTotal})" : ComponentName;


    }
}
