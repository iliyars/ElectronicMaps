using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.WPF.Features.Workspace.FormCards;
using ElectronicMaps.WPF.Services.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Wpf.Ui.Controls;

namespace ElectronicMaps.WPF.Services.Factories
{
    public class CardViewModelFactory : ICardViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CardViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Создать CardViewModel в зависимости от FormCode
        /// </summary>
        public CardViewModelBase CreateCardViewModel(ComponentDraft draft, int number)
        {
            // Определяем формат названия для всех форм
            var formTitle = GetFormTitle(draft.FormCode, draft.FormName);

            // FORM_4 - особая обработка
            if (string.Equals(draft.FormCode, "FORM_4", StringComparison.OrdinalIgnoreCase))
            {
                return new FamilyCardsViewModel(
                    draft.FormCode,
                    formTitle,
                    number,
                    draft);
            }

            // Неопределённая форма - с зависимостями
            if (string.Equals(draft.FormCode, WorkspaceViewKeys.UndefinedForm, StringComparison.OrdinalIgnoreCase))
            {
                var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
                var viewModelFactory = _serviceProvider.GetRequiredService<IViewModelFactory>();
                var logger = _serviceProvider.GetRequiredService<ILogger<UndefinedCardViewModel>>();

                return new UndefinedCardViewModel(
                    draft.FormCode,
                    formTitle,
                    number,
                    draft,
                    dialogService,
                    viewModelFactory,
                    logger);
            }

            return new ComponentCardViewModel(
                draft.FormCode,
                formTitle,
                number,
                draft);
        }

        /// <summary>
        /// Получить читаемое название формы
        /// </summary>
        private static string GetFormTitle(string formCode, string? formName)
        {
            // Если есть имя формы - используем его
            if (!string.IsNullOrWhiteSpace(formName))
                return formName;

            // Неопределённая форма
            if (string.Equals(formCode, WorkspaceViewKeys.UndefinedForm, StringComparison.OrdinalIgnoreCase))
                return "Неопределённая форма";

            // Попытка распарсить номер формы (FORM_4 → "Форма 4")
            if (formCode.StartsWith("FORM_", StringComparison.OrdinalIgnoreCase))
            {
                var tail = formCode.Substring("FORM_".Length);
                if (int.TryParse(tail, out var number))
                    return $"Форма {number}";
            }

            // Fallback - сам код
            return formCode;
        }
    }
}
