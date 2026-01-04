using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Features.Workspace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.FormCards
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

        /// <summary>
        /// ID компонента
        /// </summary>
        public Guid Id => Item.Id;


        public IAsyncRelayCommand FillCommand { get; } //TODO: Разобраться кгде должна быть эта команда


        //TODO: form enum
        public bool IsFamilyForm => string.Equals(FormCode, "FORM_4", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Тип карточки (для UI различения)
        /// </summary>
        public abstract string CardType { get; }


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


        public virtual void ReplaceItem(ComponentDraft newDraft)
        {
            Item = newDraft;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(Id));
        }
    }
}
