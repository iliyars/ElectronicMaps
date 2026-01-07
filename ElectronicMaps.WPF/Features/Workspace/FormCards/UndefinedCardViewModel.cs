using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.WPF.Services.Dialogs;
using ElectronicMaps.WPF.Services.Factories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.FormCards
{
    public partial class UndefinedCardViewModel : CardViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly ILogger<UndefinedCardViewModel> _logger;

        public override string CardType => "Undefined";

        /// <summary>
        /// Причина неопределённости (если есть)
        /// </summary>
        public string UndefinedReason => "Форма не определена";

        public UndefinedCardViewModel(
            string formCode,
            string formName,
            int number,
            ComponentDraft item,
            IDialogService dialogService,
            IViewModelFactory viewModelFactory,
            ILogger<UndefinedCardViewModel> logger)
            : base(formCode, formName, number, item)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public override void ReplaceItem(ComponentDraft newItem)
        {
            base.ReplaceItem(newItem);

            // Уведомляем об изменении специфичных свойств
        }
        [RelayCommand]
        private async Task AddToDatabaseAsync()
        {
            try
            {
                _logger.LogInformation(
                "Открытие диалога создания компонента для '{ComponentName}'",
                Item.Name);

                // 1. Создать ViewModel для диалога
                var dialogViewModel = _viewModelFactory.CreateCreateComponentViewModel();

                // 2. Инициализировать с данными текущего компонента
                await dialogViewModel.InitializeAsync(Item);

                // 3. Показать модальное окно
                var result = await _dialogService.ShowDialogAsync(dialogViewModel);

                // 4. Обработать результат
                if (result == true)
                {
                    _logger.LogInformation(
                        "Компонент '{ComponentName}' успешно сохранён",
                        Item.Name);

                    // TODO: Обновить Item с данными из БД
                    // TODO: Обновить UI (например, изменить CardType)

                    _dialogService.ShowMessage(
                        $"Компонент '{Item.Name}' успешно добавлен в базу данных!",
                        "Успех");
                }
                else
                {
                    _logger.LogInformation(
                        "Создание компонента '{ComponentName}' отменено пользователем",
                        Item.Name);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при открытии диалога создания компонента для '{ComponentName}'",
                    Item.Name);

                // Показать ошибку пользователю
                await _dialogService.ShowErrorAsync(
                    "Ошибка",
                    $"Не удалось открыть окно создания компонента: {ex.Message}");
            }
        }
    }
}
