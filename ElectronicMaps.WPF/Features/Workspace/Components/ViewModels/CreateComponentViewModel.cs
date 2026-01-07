using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Families;
using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Application.Features.Components.Services;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Domain.Enums;
using ElectronicMaps.WPF.Services.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.Components.ViewModels
{
    public partial class CreateComponentViewModel : ObservableObject
    {
        private readonly IComponentCreationService _creationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<CreateComponentViewModel> _logger;

        public CreateComponentViewModel(
       IComponentCreationService creationService,
       IDialogService dialogService,
       ILogger<CreateComponentViewModel> logger)
        {
            _creationService = creationService ?? throw new ArgumentNullException(nameof(creationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Properties

        [ObservableProperty]
        private int _currentStep = 0;

        [ObservableProperty]
        private string _componentName = "";

        [ObservableProperty]
        private string _newFamilyName = "";

        [ObservableProperty]
        private bool _isFamilyExists = false;

        [ObservableProperty]
        private ComponentFamilyLookupDto? _existingFamily;


        public ObservableCollection<ParameterDefinitionDto> FamilyParameterDefinitions { get; } = new();
        public ObservableCollection<ParameterInput> FamilyParameters { get; } = new();

        public ObservableCollection<FormTypeDto> AvailableComponentForms { get; } = new();

        [ObservableProperty]
        private FormTypeDto? _selectedComponentForm;

        public ObservableCollection<ParameterDefinitionDto> ComponentParameterDefinitions { get; } = new();
        public ObservableCollection<ParameterInput> ComponentParameters { get; } = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string? _statusMessage;

        // Для закрытия окна
        public Action? CloseAction { get; set; }
        public bool? DialogResult { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализация с данными из ComponentDraft
        /// </summary>
        public async Task InitializeAsync(ComponentDraft draft)
        {
            IsBusy = true;
            StatusMessage = "Загрузка данных...";

            try
            {
                // 1. Установить имя компонента из draft
                ComponentName = draft.Name;

                // 2. Если у draft есть FamilyKey - попробовать найти семейство
                if (!string.IsNullOrWhiteSpace(draft.Family))
                {
                    NewFamilyName = draft.Family;

                    // Проверить существует ли семейство
                    var existingFamily = await _creationService.FindFamilyByNameAsync(draft.Family);
                    if (existingFamily != null)
                    {
                        // Семейство существует - переключить на режим UseExisting
                        IsFamilyExists = true;
                        ExistingFamily = existingFamily;
                    }
                    else
                    {
                        // Семейство не существует - нужно создать
                        IsFamilyExists = false;
                    }
                }

                // 3. Загрузить параметры семейства (если нужно создать)
                if (!IsFamilyExists)
                {
                    var familyParams = await _creationService.GetFamilyParameterDefinitionsAsync();
                    FamilyParameterDefinitions.Clear();
                    foreach(var param in familyParams)
                    {
                        FamilyParameterDefinitions.Add(param);
                        FamilyParameters.Add(new ParameterInput(param));
                    }
                }

                // 4. Загрузить формы компонента
                var componentForms = await _creationService.GetAvailableComponentFormsAsync();
                AvailableComponentForms.Clear();
                foreach (var form in componentForms)
                {
                    AvailableComponentForms.Add(form);
                }
                
                StatusMessage = IsFamilyExists
                ? $"Семейство '{ExistingFamily!.Name}' найдено в БД"
                : "Готово";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка инициализации");
                StatusMessage = $"Ошибка: {ex.Message}";
                await _dialogService.ShowErrorAsync("Ошибка", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Step Navigation

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void NextStep()
        {
            if (CurrentStep == 0 && IsFamilyExists)
            {
                CurrentStep = 2;
            }
            else if(CurrentStep < 2)
            {
                CurrentStep++;
            }
            NextStepCommand.NotifyCanExecuteChanged();
            PreviousStepCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }

        private bool CanGoNext()
        {
            return CurrentStep switch
            {
                0 => !string.IsNullOrWhiteSpace(ComponentName), // Шаг 1: Имя заполнено
                1 => !string.IsNullOrWhiteSpace(NewFamilyName), // Шаг 2: Семейство заполнено
                _ => false
            };
        }

        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        private void PreviousStep()
        {
            if (CurrentStep == 2 && IsFamilyExists)
            {
                // Возврат с шага компонента сразу на имя (пропускаем семейство)
                CurrentStep = 0;
                _logger.LogDebug("Возврат на шаг имени (семейство пропущено)");
            }
            else if (CurrentStep > 0)
            {
                CurrentStep--;
                _logger.LogDebug("Возврат на шаг {Step}", CurrentStep + 1);
            }

            NextStepCommand.NotifyCanExecuteChanged();
            PreviousStepCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }

        private bool CanGoPrevious()
        {
            return CurrentStep > 0;
        }

        #endregion

        #region Component Form Selection

        partial void OnSelectedComponentFormChanged(FormTypeDto? value)
        {
            if (value == null) return;
            _ = LoadComponentParametersAsync(value.Code);
        }

        private async Task LoadComponentParametersAsync(string formTypeCode)
        {
            IsBusy = true;
            StatusMessage = $"Загрузка параметров...";

            try
            {
                var parameters = await _creationService.GetComponentParameterDefinitionsAsync(formTypeCode);

                ComponentParameterDefinitions.Clear();
                ComponentParameters.Clear();

                foreach (var param in parameters)
                {
                    ComponentParameterDefinitions.Add(param);
                    ComponentParameters.Add(new ParameterInput(param));
                }

                StatusMessage = $"Параметры загружены ({parameters.Count} шт.)";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки параметров");
                StatusMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion

  
        #region Save Command

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            IsBusy = true;
            StatusMessage = "Сохранение...";

            try
            {
                var request = CreateRequest();

                var validationResult = await _creationService.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join("\n", validationResult.Errors);
                    StatusMessage = $"Ошибка валидации";
                    await _dialogService.ShowErrorAsync("Ошибка валидации", errors);
                    return;
                }

                var result = await _creationService.CreateComponentAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Компонент '{Name}' создан", ComponentName);
                    StatusMessage = "Компонент успешно сохранён!";

                    DialogResult = true;
                    CloseAction?.Invoke();
                }
                else
                {
                    _logger.LogWarning("Ошибка сохранения: {Error}", result.ErrorMessage);
                    await _dialogService.ShowErrorAsync("Ошибка", result.ErrorMessage ?? "Неизвестная ошибка");
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения");
                await _dialogService.ShowErrorAsync("Ошибка", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSave()
        {
            return !IsBusy
                && CurrentStep == 2  // Только на последнем шаге
                && !string.IsNullOrWhiteSpace(ComponentName)
                && (IsFamilyExists || !string.IsNullOrWhiteSpace(NewFamilyName))  // Семейство есть или имя заполнено
                && SelectedComponentForm != null;
        }

        private CreateComponentRequest CreateRequest()
        {
            if (IsFamilyExists)
            {
                // Используем существующее семейство
                return new CreateComponentRequest
                {
                    ComponentName = ComponentName,
                    FamilyMode = FamilySelectionMode.UseExisting,
                    NewFamilyName = null,
                    ExistingFamilyId = ExistingFamily!.Id,
                    FamilyParameters = null,
                    ComponentFormTypeCode = SelectedComponentForm!.Code,
                    ComponentParameters = ComponentParameters.Select(p => p.ToParameterValueInput()).ToList()
                };
            }
            else
            {
                // Создаём новое семейство
                return new CreateComponentRequest
                {
                    ComponentName = ComponentName,
                    FamilyMode = FamilySelectionMode.CreateNew,
                    NewFamilyName = NewFamilyName,
                    ExistingFamilyId = null,
                    FamilyParameters = FamilyParameters.Select(p => p.ToParameterValueInput()).ToList(),
                    ComponentFormTypeCode = SelectedComponentForm!.Code,
                    ComponentParameters = ComponentParameters.Select(p => p.ToParameterValueInput()).ToList()
                };
            }
        }

        #endregion

        #region Cancel Command

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
            CloseAction?.Invoke();
        }
        #endregion

        #region Helper Classes
        public partial class ParameterInput : ObservableObject
        {
            private readonly ParameterDefinitionDto _definition;

            public ParameterInput(ParameterDefinitionDto definition)
            {
                _definition = definition;
            }

            public string Code => _definition.Code;
            public string DisplayName => _definition.DisplayName;
            public string? Unit => _definition.Unit;
            public ParameterValueKind ValueKind => _definition.DataType;

            [ObservableProperty]
            private string? _value;

            [ObservableProperty]
            private string? _pins;

            public ParameterValueInput ToParameterValueInput()
            {
                return new ParameterValueInput
                {
                    ParameterCode = Code,
                    Value = Value,
                    Pins = Pins
                };
            }
        }

        #endregion
    }
}