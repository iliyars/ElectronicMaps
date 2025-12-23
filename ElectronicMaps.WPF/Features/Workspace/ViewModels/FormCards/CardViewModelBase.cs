using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.WorkspaceProject.Models;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.GridRows;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.Modal;
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


        public IAsyncRelayCommand FillCommand { get; } //TODO: Разобраться кгде должна быть эта команда


        //TODO: form enum
        public bool IsFamilyForm => string.Equals(FormCode, "FORM_4", StringComparison.OrdinalIgnoreCase);




        protected CardViewModelBase(string formCode, string formName, int number, ComponentDraft item)
        {
            FormCode = formCode ?? throw new ArgumentNullException(nameof(formCode));
            FormName = formName ?? throw new ArgumentNullException(nameof(formName));
            Number = number;
            Item = item;


            //FillCommand = new AsyncRelayCommand(async () =>
            //{
            //    var vm = new UndefinedComponentFillWizardViewModel(Item.Id, fillService);
            //    await vm.InitializeAsync(CancellationToken.None);
            //    await dialogs.ShowDialogAsync(vm); // как у тебя устроены экраны/модалки
            //});
        }


        public void ReplaceItem(ComponentDraft newDraft)
        {

        }
    }
}
