using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Features.Workspace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.FormCards
{
    public class FamilyCardsViewModel : CardViewModelBase
    {
        /// Название семейства
        /// </summary>
        public string FamilyName => Item.Family ?? "Без семейства";



        public override string CardType => "Family";

        public FamilyCardsViewModel(string formCode, string formName, int number, ComponentDraft item, IRelayCommand<Guid> toggleDetailsCommand) : base(formCode, formName, number, item, toggleDetailsCommand)
        {
        }

        public override void ReplaceItem(ComponentDraft newItem)
        {
            base.ReplaceItem(newItem);

            // Уведомляем об изменении специфичных свойств
            OnPropertyChanged(nameof(FamilyName));
        }
    }
}
