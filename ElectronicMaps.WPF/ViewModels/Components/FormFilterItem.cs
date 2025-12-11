using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.ViewModels.Components
{
    public sealed class FormFilterItem : ObservableObject
    {
        public string Code { get; }
        public string DisplayName { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if(SetProperty(ref _isSelected, value))
                {
                    OnSelectionChanged?.Invoke(this);
                }

            }
        }

        public Action<FormFilterItem> OnSelectionChanged { get; set; }

        public FormFilterItem(string code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }
    }
}
