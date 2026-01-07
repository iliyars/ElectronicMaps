using DocumentFormat.OpenXml.Office.CustomUI;
using ElectronicMaps.Application.Abstractions.Commands;
using ElectronicMaps.Application.Abstractions.Queries.Components;
using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.Abstractions.Queries.Forms;
using ElectronicMaps.Application.Abstractions.Queries.Parameters;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Families;
using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Application.Features.Workspace.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Components.Services
{
    public class ComponentCreationService : IComponentCreationService
    {
        private readonly ISaveComponent _saveComponent;
        private readonly IFormTypeReadRepository _formTypes;
        private readonly IParameterDefinitionReadRepository _parameterDefinitions;
        private readonly IComponentFamilyReadRepository _families;
        private readonly IComponentReadRepository _components;
        private readonly ILogger<ComponentCreationService> _logger;

        // Код формы семейства (константа)
        private const string FamilyFormTypeCode = WorkspaceViewKeys.FamilyFormCode;

        public ComponentCreationService(ISaveComponent saveComponent, IFormTypeReadRepository formTypes, IParameterDefinitionReadRepository parameterDefinitions, IComponentFamilyReadRepository families, IComponentReadRepository components, ILogger<ComponentCreationService> logger)
        {
            _saveComponent = saveComponent;
            _formTypes = formTypes;
            _parameterDefinitions = parameterDefinitions;
            _families = families;
            _components = components;
            _logger = logger;
        }

        #region Получение даных из UI

        // <summary>
        /// Получить параметры семейства для заполнения
        /// Вызывается при открытии модального окна
        /// </summary>
        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetFamilyParameterDefinitionsAsync(
            CancellationToken ct = default)
        {
            _logger.LogDebug("Получение параметров семейства для FormType: {FormTypeCode}", FamilyFormTypeCode);

            try
            {
                var familyFormType = await _formTypes.GetByCodeAsync(FamilyFormTypeCode, ct);

                var parameters = await _parameterDefinitions.GetByFormCodeAsync(FamilyFormTypeCode, ct);

                _logger.LogInformation(
                "Получено {Count} параметров семейства",
                parameters.Count);

                return parameters;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при получении параметров семейства для FormType: {FormTypeCode}",
                    FamilyFormTypeCode);
                throw;
            }
        }

        //TODO: Список ворм будет ENUM 
        /// <summary>
        /// Получить список доступных форм компонента
        /// Вызывается для заполнения ComboBox выбора формы
        /// </summary>
        public async Task<IReadOnlyList<FormTypeDto>> GetAvailableComponentFormsAsync(
            CancellationToken ct = default)
        {
            _logger.LogDebug("Получение списка доступных форм компонента");

            try
            {
                var formTypes = await _formTypes.GetAllAsync(ct);

                _logger.LogInformation(
                    "Получено {Count} форм компонента",
                    formTypes.Count);

                return formTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка форм компонента");
                throw;
            }
        }

        // <summary>
        /// Получить параметры компонента для заполнения
        /// Вызывается после выбора FormType в ComboBox
        /// </summary>
        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetComponentParameterDefinitionsAsync(
            string formTypeCode,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(formTypeCode))
            {
                throw new ArgumentException("Код формы не может быть пустым", nameof(formTypeCode));
            }

            _logger.LogDebug(
                "Получение параметров компонента для FormType: {FormTypeCode}",
                formTypeCode);

            try
            {
                // Получаем FormType компонента
                var componentFormType = await _formTypes.GetByCodeAsync(formTypeCode, ct);

                // Получаем параметры этой формы (упорядочены по Order)
                var parameters = await _parameterDefinitions.GetByFormTypeIdAsync(componentFormType.Id, ct);

                _logger.LogInformation(
               "Получено {Count} параметров для формы {FormTypeCode}",
               parameters.Count,
               formTypeCode);

                return parameters;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при получении параметров для FormType: {FormTypeCode}",
                    formTypeCode);
                throw;
            }
        }

        #endregion

        #region Работа с семейством

        /// <summary>
        /// Найти семейство по имени
        /// Используется для проверки существования семейства перед созданием
        /// </summary>
        public async Task<ComponentFamilyLookupDto?> FindFamilyByNameAsync(
            string familyName,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(familyName))
            {
                throw new ArgumentException("Имя семейства не может быть пустым", nameof(familyName));
            }

            _logger.LogDebug("Поиск семейства по имени: {FamilyName}", familyName);

            try
            {
                var family = await _families.FindByNameAsync(familyName, ct);

                if (family == null)
                {
                    _logger.LogDebug("Семейство '{FamilyName}' не найдено", familyName);
                    return null;
                }

                _logger.LogDebug(
                "Семейство найдено: Id={FamilyId}, Name={FamilyName}",
                family.Id,
                family.Name);

                return new ComponentFamilyLookupDto
                {
                    Id = family.Id,
                    Name = family.Name,
                    ComponentCount = family.ComponentCount,
                    FormTypeName = family.FormTypeName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при поиске семейства по имени: {FamilyName}",
                    familyName);
                throw;
            }
        }

        /// <summary>
        /// Получить список всех семейств
        /// Используется для выбора существующего семейства в UI
        /// </summary>
        public async Task<IReadOnlyList<ComponentFamilyLookupDto>> GetAllFamiliesAsync(
            CancellationToken ct = default)
        {
            _logger.LogDebug("Получение списка всех семейств");

            try
            {
                var families = await _families.GetAllAsync(ct);

                var lookupDtos = families.Select(f => new ComponentFamilyLookupDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    ComponentCount = f.ComponentCount,
                    FormTypeName = f.FormTypeName
                }).ToList();

                _logger.LogInformation("Получено {Count} семейств", lookupDtos.Count);

                return lookupDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка семейств");
                throw;
            }
        }
        #endregion

        #region Валидация

        /// <summary>
        /// Валидировать данные перед сохранением
        /// </summary>
        public async Task<ComponentCreationValidationResult> ValidateAsync(
            CreateComponentRequest request,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogDebug("Валидация запроса на создание компонента: {ComponentName}", request.ComponentName);

            var errors = new List<string>();
            var warnings = new List<string>();

            // 1. Обязательные поля
            if (string.IsNullOrWhiteSpace(request.ComponentName))
            {
                errors.Add("Имя компонента обязательно для заполнения");
            }

            if (string.IsNullOrWhiteSpace(request.ComponentFormTypeCode))
            {
                errors.Add("Необходимо выбрать форму компонента");
            }

            // 2. Семейство
            if (request.FamilyMode == FamilySelectionMode.UseExisting)
            {
                if (!request.ExistingFamilyId.HasValue)
                {
                    errors.Add("Необходимо выбрать существующее семейство");
                }
                else
                {
                    // Проверяем существование семейства
                    var familyExists = await _families.ExistsAsync(request.ExistingFamilyId.Value, ct);
                    if (!familyExists)
                    {
                        errors.Add($"Семейство с ID={request.ExistingFamilyId.Value} не найдено");
                    }
                }
            }
            else if (request.FamilyMode == FamilySelectionMode.CreateNew)
            {
                if (string.IsNullOrWhiteSpace(request.NewFamilyName))
                {
                    errors.Add("Необходимо указать имя нового семейства");
                }
                else
                {
                    // Проверяем не существует ли уже семейство с таким именем
                    var existingFamily = await FindFamilyByNameAsync(request.NewFamilyName, ct);
                    if (existingFamily != null)
                    {
                        warnings.Add(
                            $"Семейство '{request.NewFamilyName}' уже существует. " +
                            $"Компонент будет добавлен в существующее семейство (ID={existingFamily.Id}).");
                    }
                }
            }

            // 3. Уникальность имени компонента
            if (!string.IsNullOrWhiteSpace(request.ComponentName))
            {
                var componentExists = await _components.ExistsAsync(request.ComponentName, ct);
                if (componentExists)
                {
                    errors.Add($"Компонент с именем '{request.ComponentName}' уже существует");
                }
            }

            // 4. FormType существует
            if (!string.IsNullOrWhiteSpace(request.ComponentFormTypeCode))
            {
                try
                {
                    await _formTypes.GetByCodeAsync(request.ComponentFormTypeCode, ct);
                }
                catch
                {
                    errors.Add($"Форма '{request.ComponentFormTypeCode}' не найдена");
                }
            }

            var result = errors.Count == 0
            ? new ComponentCreationValidationResult
            {
                IsValid = true,
                Warnings = warnings
            }
            : new ComponentCreationValidationResult
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings
            };

            if (!result.IsValid)
            {
                _logger.LogWarning(
                    "Валидация не пройдена для компонента '{ComponentName}': {Errors}",
                    request.ComponentName,
                    string.Join("; ", errors));
            }
            else if (warnings.Count > 0)
            {
                _logger.LogInformation(
                    "Валидация пройдена с предупреждениями для '{ComponentName}': {Warnings}",
                    request.ComponentName,
                    string.Join("; ", warnings));
            }
            else
            {
                _logger.LogDebug("Валидация успешно пройдена для '{ComponentName}'", request.ComponentName);
            }

            return result;

        }

        #endregion

        #region 4. Создание компонента

        /// <summary>
        /// Создать компонент
        /// </summary>
        public async Task<ComponentCreationResult> CreateComponentAsync(
            CreateComponentRequest request,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogInformation(
           "Начало создания компонента '{ComponentName}' (FormType: {FormType}, FamilyMode: {FamilyMode})",
           request.ComponentName,
           request.ComponentFormTypeCode,
           request.FamilyMode);

            try
            {
                // 1. Валидация
                var validationResult = await ValidateAsync(request, ct);
                if (!validationResult.IsValid)
                {
                    var errorMessage = string.Join(";", validationResult.Errors);
                    _logger.LogWarning(
                    "Создание компонента '{ComponentName}' отменено: валидация не пройдена",
                    request.ComponentName);

                    return ComponentCreationResult.Failure(errorMessage);
                }

                // 2. Подготовка SaveComponentRequest
                var saveRequest = MapToSaveComponentRequest(request);

                // 3. Сохранение через ISaveComponent
                var saveResult = await _saveComponent.SaveAsync(saveRequest, ct);

                if (!saveResult.IsSuccess)
                {
                    _logger.LogError(
                    "Ошибка при сохранении компонента '{ComponentName}': {ErrorMessage}",
                    request.ComponentName,
                    saveResult.ErrorMessage);

                    return ComponentCreationResult.Failure(
                    saveResult.ErrorMessage ?? "Неизвестная ошибка при сохранении");
                }

                // 4. Успех
                _logger.LogInformation(
                    "Компонент '{ComponentName}' успешно создан: ComponentId={ComponentId}, FamilyId={FamilyId}, FamilyWasCreated={FamilyWasCreated}",
                    request.ComponentName,
                    saveResult.ComponentId,
                    saveResult.ComponentFamilyId,
                    saveResult.FamilyWasCreated);

                return ComponentCreationResult.Success(
                    saveResult.ComponentId,
                    saveResult.ComponentFamilyId,
                    saveResult.FamilyWasCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Неожиданная ошибка при создании компонента '{ComponentName}'",
                    request.ComponentName);

                return ComponentCreationResult.Failure($"Неожиданная ошибка: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Преобразовать CreateComponentRequest в SaveComponentRequest
        /// </summary>
        private SaveComponentRequest MapToSaveComponentRequest(CreateComponentRequest request)
        {
            return new SaveComponentRequest
            {
                ComponentName = request.ComponentName,
                ComponentFormTypeCode = request.ComponentFormTypeCode,
                ComponentParameters = request.ComponentParameters ?? [],

                // Семейство
                ExistingFamilyId = request.FamilyMode == FamilySelectionMode.UseExisting
                ? request.ExistingFamilyId
                : null,

                FamilyName = request.FamilyMode == FamilySelectionMode.CreateNew
                ? request.NewFamilyName
                : null,

                FamilyFormTypeCode = request.FamilyMode == FamilySelectionMode.CreateNew
                ? FamilyFormTypeCode
                : null,

                FamilyParameters = request.FamilyMode == FamilySelectionMode.CreateNew
                ? request.FamilyParameters ?? []
                : null,

                // Аудит
                CreatedByUserId = request.UserId

            };
        }

        #endregion

    }
}
