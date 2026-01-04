using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Features.Workspace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.FormCards
{
    public partial class UndefinedCardViewModel : CardViewModelBase
    {
        public override string CardType => "Undefined";

        /// <summary>
        /// Причина неопределённости (если есть)
        /// </summary>
        public string UndefinedReason => "Форма не определена";

        public UndefinedCardViewModel(string formCode, string formName, int number, ComponentDraft item) : base(formCode, formName, number, item)
        {
        }


        public override void ReplaceItem(ComponentDraft newItem)
        {
            base.ReplaceItem(newItem);

            // Уведомляем об изменении специфичных свойств
        }
        [RelayCommand]
        private void AddToDatabase()
        {

        }
    }
}
