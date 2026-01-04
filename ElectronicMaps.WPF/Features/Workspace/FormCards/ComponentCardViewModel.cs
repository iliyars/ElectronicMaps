using ElectronicMaps.Application.Features.Workspace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.FormCards
{
    public partial class ComponentCardViewModel : CardViewModelBase
    {
        public ComponentCardViewModel(string formCode, string formName, int number, ComponentDraft item)
           : base(formCode, formName, number, item)
        {

        }

        public override string CardType => "Component";

        /// <summary>
        /// Позиционные обозначения (R1, C2, U3, etc.)
        /// </summary>
        public string Designators => Item.Designators != null && Item.Designators.Count > 0
            ? string.Join(", ", Item.Designators)
            : "—";


        public override void ReplaceItem(ComponentDraft newItem)
        {
            base.ReplaceItem(newItem);

            // Уведомляем об изменении специфичных свойств
            OnPropertyChanged(nameof(Designators));
        }
    }
}
