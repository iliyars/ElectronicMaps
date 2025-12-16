using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.GridRows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Features.Workspace.ViewModels.FormCards
{
    public abstract class CardViewModelBase : ObservableObject
    {
        public string FormCode { get; }
        public string FormName { get; }
        private int _number;
        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }
        public ObservableCollection<CardItemViewModel> Items { get; } = new();

        protected CardViewModelBase(string formCode, string formName, int number, ObservableCollection<CardItemViewModel> items)
        {
            FormCode = formCode ?? throw new ArgumentNullException(nameof(formCode));
            FormName = formName ?? throw new ArgumentNullException(nameof(formName));
        }

        public bool IsFamilyForm => string.Equals(FormCode, "FORM_4", StringComparison.OrdinalIgnoreCase);
    }
}
