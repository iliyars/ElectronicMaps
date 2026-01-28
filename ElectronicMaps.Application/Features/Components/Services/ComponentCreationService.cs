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
        private readonly IComponentFamilyQueryService _familyQuery;
        private readonly IComponentReadRepository _components;
        private readonly ILogger<ComponentCreationService> _logger;

        // Код формы семейства (константа)
        private const string FamilyFormTypeCode = WorkspaceViewKeys.FamilyFormCode;

        public ComponentCreationService(
            ISaveComponent saveComponent,
            IFormTypeReadRepository formTypes,
            IComponentFamilyQueryService familyQuery,
            IComponentReadRepository components,
            ILogger<ComponentCreationService> logger)
        {
            _saveComponent = saveComponent;
            _formTypes = formTypes;
            _familyQuery = familyQuery;
            _components = components;
            _logger = logger;
        }


        /// <summary>
        /// Валидация запроса на создание компонента
        /// </summary>
        public async Task<ComponentCreationValidationResult> ValidateAsync(
            CreateComponentRequest request,
            CancellationToken ct = default)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // 1. Имя компонента
            if (string.IsNullOrWhiteSpace(request.ComponentName))
            {
                errors.Add("Необходимо указать имя компонента");
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
                    var familyExists = await _familyQuery.GetAllFamiliesAsync(ct);
                    if (!familyExists.Any(f => f.Id == request.ExistingFamilyId.Value))
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
                    var existingFamily = await _familyQuery.FindFamilyByNameAsync(request.NewFamilyName, ct);
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

            return result;
        }

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
                var validation = await ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    var errorMessage = string.Join(";", validation.Errors);
                    _logger.LogWarning(
                        "Создание компонента '{ComponentName}' отменено: валидация не пройдена",
                        request.ComponentName);
                    return ComponentCreationResult.Failure(errorMessage);
                }

                // 2. Подготовка SaveComponentRequest
                var saveRequest = MapToSaveComponentRequest(request);

                // 3. Сохранение через ISaveComponent
                var saveResult = await _saveComponent.SaveAsync(saveRequest, ct);

                if(!saveResult.IsSuccess)
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
                    "Компонент '{ComponentName}' успешно создан: ComponentId={ComponentId}, FamilyId={FamilyId}",
                    request.ComponentName,
                    saveResult.ComponentId,
                    saveResult.ComponentFamilyId);

                return ComponentCreationResult.Success(
                    saveResult.ComponentId,
                    saveResult.ComponentFamilyId,
                    saveResult.FamilyWasCreated);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Неожиданная ошибка при создании компонента '{ComponentName}'",
                    request.ComponentName);

                return ComponentCreationResult.Failure($"Неожиданная ошибка: {ex.Message}");
            }
        }

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
    }
}
