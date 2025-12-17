using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Application.WorkspaceProject.Models;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.GridRows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Features.Workspace.ViewModels.FormCards
{
    public abstract partial class CardViewModelBase : ObservableObject
    {
        public string FormCode { get; }
        public string FormName { get; }

        [ObservableProperty]
        private int number;

        [ObservableProperty]
        private bool isDetailsOpen;

        [ObservableProperty]
        private ComponentDraft item;

        public string Name => item.Name;
        public int Quantity => item.Quantity;


        //TODO: form enum
        public bool IsFamilyForm => string.Equals(FormCode, "FORM_4", StringComparison.OrdinalIgnoreCase);




        protected CardViewModelBase(string formCode, string formName, int number, ComponentDraft item)
        {
            FormCode = formCode ?? throw new ArgumentNullException(nameof(formCode));
            FormName = formName ?? throw new ArgumentNullException(nameof(formName));
            Number = number;
            Item = item;
        }


        public void ReplaceItem(ComponentDraft newDraft)
        {

        }
    }
}
