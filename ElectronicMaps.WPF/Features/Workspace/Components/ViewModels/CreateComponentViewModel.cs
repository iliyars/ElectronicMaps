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
        private string _componentName = "";

        [ObservableProperty]
        private bool _isNewFamily = true;

        [ObservableProperty]
        private bool _isExistingFamily = false;

        [ObservableProperty]
        private string _newFamilyName = "";

        [ObservableProperty]
        private ComponentFamilyLookupDto? _selectedFamily;

        public ObservableCollection<ComponentFamilyLookupDto> AvailableFamilies { get; } = new();

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
                if (!string.IsNullOrWhiteSpace(draft.FamilyKey))
                {
                    NewFamilyName = draft.FamilyKey;

                    // Проверить существует ли семейство
                    var existingFamily = await _creationService.FindFamilyByNameAsync(draft.FamilyKey);
                    if (existingFamily != null)
                    {
                        // Семейство существует - переключить на режим UseExisting
                        IsNewFamily = false;
                        IsExistingFamily = true;
                        SelectedFamily = existingFamily;
                    }
                }

                // 3. Загрузить параметры семейства
                var familyParams = await _creationService.GetFamilyParameterDefinitionsAsync();
                FamilyParameterDefinitions.Clear();
                foreach (var param in familyParams)
                {
                    FamilyParameterDefinitions.Add(param);
                    FamilyParameters.Add(new ParameterInput(param));
                }

                // 4. Загрузить формы компонента
                var componentForms = await _creationService.GetAvailableComponentFormsAsync();
                AvailableComponentForms.Clear();
                foreach (var form in componentForms)
                {
                    AvailableComponentForms.Add(form);
                }

                // 5. Загрузить список семейств
                var families = await _creationService.GetAllFamiliesAsync();
                AvailableFamilies.Clear();
                foreach (var family in families)
                {
                    AvailableFamilies.Add(family);
                }

                StatusMessage = "Готово к заполнению";
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

        #region Family Mode
        [RelayCommand]
        private void SelectNewFamily()
        {
            IsNewFamily = true;
            IsExistingFamily = false;
            SelectedFamily = null;
        }

        [RelayCommand]
        private void SelectExistingFamily()
        {
            IsNewFamily = false;
            IsExistingFamily = true;
            NewFamilyName = "";
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
                && !string.IsNullOrWhiteSpace(ComponentName)
                && SelectedComponentForm != null
                && (IsNewFamily ? !string.IsNullOrWhiteSpace(NewFamilyName) : SelectedFamily != null);
        }

        private CreateComponentRequest CreateRequest()
        {
            return new CreateComponentRequest()
            {
                ComponentName = ComponentName,

                FamilyMode = IsNewFamily
                    ? FamilySelectionMode.CreateNew
                    : FamilySelectionMode.UseExisting,

                NewFamilyName = IsNewFamily ? NewFamilyName : null,
                ExistingFamilyId = IsExistingFamily ? SelectedFamily?.Id : null,

                FamilyParameters = IsNewFamily
                    ? FamilyParameters.Select(p => p.ToParameterValueInput()).ToList()
                    : null,

                ComponentFormTypeCode = SelectedComponentForm!.Code,
                ComponentParameters = ComponentParameters.Select(p => p.ToParameterValueInput()).ToList()
            };
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