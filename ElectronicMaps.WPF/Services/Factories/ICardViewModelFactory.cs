using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.WPF.Features.Workspace.FormCards;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Services.Factories
{
    public interface ICardViewModelFactory
    {
        /// <summary>
        /// Создать CardViewModel для компонента
        /// </summary>
        CardViewModelBase CreateCardViewModel(ComponentDraft draft, int number);
    }
}
