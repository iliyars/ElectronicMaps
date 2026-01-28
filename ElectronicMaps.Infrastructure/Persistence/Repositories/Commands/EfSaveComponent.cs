using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using ElectronicMaps.Application.Abstractions.Commands;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Enums;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Commands
{
    public class EfSaveComponent : ISaveComponent
    {
        private readonly AppDbContext _db;
        private readonly ILogger<EfSaveComponent> _logger;

        public EfSaveComponent(AppDbContext db, ILogger<EfSaveComponent> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SaveComponentResult> SaveAsync(
            SaveComponentRequest request,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogInformation(
                "Начало сохранения компонента '{ComponentName}' в семейство '{FamilyName}'",
                request.ComponentName,
                request.FamilyName ?? $"ID={request.ExistingFamilyId}");

            // Валидация входных данных
            ValidateRequest(request);

            // Используем транзакцию для атомарности операции
            await using var transaction = await _db.Database.BeginTransactionAsync(ct); // Открываем транзакцию, чтобы всё сохранилось атомарно
            try
            {
                // 1. Найти или создать семейство
                var (family, familyWasCreated) = await GetOrCreateFamilyAsync(request, ct);

                _logger.LogDebug(
                    "Семейство определено: Id={FamilyId}, Name={FamilyName}, IsNew={IsNew}",
                    family.Id,
                    family.Name,
                    family.Id == 0);

                // 2. Проверить что компонент с таким именем не существует
                await EnsureComponentNotExistsAsync(request.ComponentName, ct);

                // 3. Получить FormType компонента
                var componentFormType = await GetFormTypeAsync(request.ComponentFormTypeCode, "компонента", ct);

                _logger.LogDebug(
                   "FormType компонента: Id={FormTypeId}, Code={FormTypeCode}",
                   componentFormType.Id,
                   componentFormType.Code);

                // 4. Создать компонент
                var component = CreateComponent(request, family, componentFormType);

                _db.Components.Add(component);

                // 5. Сохранить изменения, чтобы получить IDs
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Компонент создан: Id={ComponentId}, Name={ComponentName}, FamilyId={FamilyId}",
                    component.Id,
                    component.Name,
                    family.Id);

                // 6. Сохранить параметры семейства (если есть и семейство новое)
                if (family.Id > 0 && request.FamilyParameters?.Count > 0)
                {
                    await SaveFamilyParametersAsync(
                        family,
                        request.FamilyParameters,
                        request.FamilyFormTypeCode,
                        ct);
                }

                // 7. Сохранить параметры компонента
                if (request.ComponentParameters?.Count > 0)
                {
                    await SaveComponentParametersAsync(
                        component,
                        request.ComponentParameters,
                        componentFormType,
                        ct);
                }

                // 8. Финальное сохранение параметров
                await _db.SaveChangesAsync(ct);

                // 9. Коммит транзакции
                await transaction.CommitAsync(ct);

                _logger.LogInformation(
                   "Компонент '{ComponentName}' успешно сохранён. ComponentId={ComponentId}, FamilyId={FamilyId}",
                   component.Name,
                   component.Id,
                   family.Id);

                return new SaveComponentResult
                {
                    ComponentId = component.Id,
                    ComponentFamilyId = family.Id,
                    IsSuccess = true,
                    FamilyWasCreated = familyWasCreated, 
                    ComponentVerificationStatus = component.VerificationStatus, 
                    FamilyVerificationStatus = family.VerificationStatus  
                };

            }
            catch (Exception ex)
            {
                // Откат транзакции при ошибке
                await transaction.RollbackAsync(ct);

                _logger.LogError(
                   ex,
                   "Ошибка при сохранении компонента '{ComponentName}': {ErrorMessage}",
                   request.ComponentName,
                   ex.Message);

                // Пробрасываем бизнес-ошибки как есть
                if (ex is ArgumentException or InvalidOperationException)
                {
                    throw;  
                }

                throw new InvalidOperationException(
                   $"Не удалось сохранить компонент '{request.ComponentName}': {ex.Message}",
                   ex);
            }
        }


        #region Validation
        private void ValidateRequest(SaveComponentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ComponentName))
            {
                throw new ArgumentException(
                    "Имя компонента обязательно для заполнения",
                    nameof(request.ComponentName));
            }

            if (string.IsNullOrWhiteSpace(request.ComponentFormTypeCode))
            {
                throw new ArgumentException(
                    "Код формы компонента обязателен для заполнения",
                    nameof(request.ComponentFormTypeCode));
            }
            // Должно быть указано либо ExistingFamilyId, либо FamilyName
            if (request.ExistingFamilyId == null && string.IsNullOrWhiteSpace(request.FamilyName))
            {
                throw new ArgumentException(
                    "Необходимо указать либо ID существующего семейства, либо имя нового семейства");
            }

            // Если создаём новое семейство, нужен FamilyFormTypeCode
            if (request.ExistingFamilyId == null && string.IsNullOrWhiteSpace(request.FamilyFormTypeCode))
            {
                throw new ArgumentException(
                    "Для нового семейства необходимо указать код формы семейства",
                    nameof(request.FamilyFormTypeCode));
            }

            _logger.LogDebug("Валидация запроса пройдена успешно");

        }

        // <summary>
        /// Проверка что компонент с таким именем не существует
        /// </summary>
        private async Task EnsureComponentNotExistsAsync(string componentName, CancellationToken ct)
        {
            var exists = await _db.Components
                .AnyAsync(c => c.Name == componentName, ct);

            if (exists)
            {
                _logger.LogWarning(
                    "Попытка создать компонент с существующим именем: {ComponentName}",
                    componentName);

                throw new InvalidOperationException(
                    $"Компонент с именем '{componentName}' уже существует в БД");
            }
        }
        #endregion

        #region Работа с семейстовом (Family)

        private async Task<(ComponentFamily, bool WasCreated)> GetOrCreateFamilyAsync(
            SaveComponentRequest request,
            CancellationToken ct)
        {
            // Случай 1: Используем существующее семейство по ID
            if (request.ExistingFamilyId.HasValue)
            {
                _logger.LogDebug(
                    "Поиск существующего семейства по ID={FamilyId}",
                    request.ExistingFamilyId.Value);

                var existingFamily = await _db.ComponentFamilies
                    .FirstOrDefaultAsync(f => f.Id == request.ExistingFamilyId.Value, ct);

                if (existingFamily == null)
                {
                    throw new InvalidOperationException(
                        $"Семейство с ID={request.ExistingFamilyId.Value} не найдено в БД");
                }

                _logger.LogDebug(
                   "Найдено существующее семейство: {FamilyName}",
                   existingFamily.Name);

                return (existingFamily, WasCreated: false);  // ✅ ИСПРАВЛЕН
            }

            // Случай 2: Создаём новое семейство
            _logger.LogDebug(
                "Создание нового семейства: {FamilyName}",
                request.FamilyName);

            // Проверяем не существует ли уже семейство с таким именем
            var duplicateFamily = await _db.ComponentFamilies
                .FirstOrDefaultAsync(f => f.Name == request.FamilyName, ct);

            if (duplicateFamily != null)
            {
                _logger.LogWarning(
                    "Семейство с именем '{FamilyName}' уже существует (ID={FamilyId}). Используем существующее.",
                    request.FamilyName,
                    duplicateFamily.Id);

                return (duplicateFamily, WasCreated: false);
            }

            // Получаем FormType для семейства
            var familyFormType = await GetFormTypeAsync(
                request.FamilyFormTypeCode!,
                "семейства",
                ct);

            // Создаём новое семейство
            var newFamily = new ComponentFamily
            {
                Name = request.FamilyName!,
                FamilyFormTypeId = familyFormType.Id,
                VerificationStatus = VerificationStatus.Unverified,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedByUserId = request.CreatedByUserId,
                Version = 1
            };

            _db.ComponentFamilies.Add(newFamily);

            // Сохраняем чтобы получить ID
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Создано новое семейство: Id={FamilyId}, Name={FamilyName}, FormTypeId={FormTypeId}",
                newFamily.Id,
                newFamily.Name,
                newFamily.FamilyFormTypeId);

            return (newFamily, WasCreated: true);

        }

        #endregion

        #region Работа с FormType

        private async Task<FormType> GetFormTypeAsync(
            string formTypeCode,
            string entityTypeName,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(formTypeCode))
            {
                throw new ArgumentException(
                    $"Код формы {entityTypeName} не может быть пустым",
                    nameof(formTypeCode));
            }

            var formType = await _db.FormTypes
                .Include(ft => ft.Parameters) // Включаем параметры для последющего использования
                .FirstOrDefaultAsync(ft => ft.Code == formTypeCode, ct);

            if(formType == null)
            {
                throw new InvalidOperationException(
                   $"FormType с кодом '{formTypeCode}' не найден в БД");
            }

            _logger.LogDebug(
               "Найден FormType: Code={Code}, DisplayName={DisplayName}, ParameterCount={ParameterCount}",
               formType.Code,
               formType.DisplayName,
               formType.Parameters.Count);

            return formType;
        }

        #endregion

        #region Создание компонента

        /// <summary>
        /// Создать сущность Component
        /// </summary>
        private Component CreateComponent(
            SaveComponentRequest request,
            ComponentFamily family,
            FormType formType)
        {
            _logger.LogDebug(
                "Создание компонента '{ComponentName}' в семействе Id={FamilyId}",
                request.ComponentName,
                family.Id);

            var component = new Component
            {
                Name = request.ComponentName,
                ComponentFamilyId = family.Id,
                FormTypeId = formType.Id,
                VerificationStatus = VerificationStatus.Unverified,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedByUserId = request.CreatedByUserId,
                Version = 1
            };

            return component;
        }

        #endregion

        #region Сохранение параметров

        /// <summary>
        /// Сохранить параметры семейства
        /// </summary>
        private async Task SaveFamilyParametersAsync(
            ComponentFamily family,
            IReadOnlyList<ParameterValueInput> parameters,
            string? familyFormTypeCode,
            CancellationToken ct)
        {
            if (parameters == null || parameters.Count == 0)
            {
                _logger.LogDebug("Параметры семейства отсутствуют, пропускаем сохранение");
                return;
            }

            _logger.LogInformation(
               "Сохранение {Count} параметров для семейства Id={FamilyId}",
               parameters.Count,
               family.Id);

            // Получаем FormType семейства чтобы знать какие параметры доступны
            var formType = await _db.FormTypes
                .Include(ft => ft.Parameters)
                .FirstOrDefaultAsync(ft => ft.Id == family.FamilyFormTypeId, ct);

            if (formType == null)
            {
                throw new InvalidOperationException(
                    $"FormType с ID={family.FamilyFormTypeId} не найден");
            }

            // Создаём ParameterValue для каждого входного параметра
            foreach(var input in parameters)
            {
                var parameterValue = await CreateParameterValueAsync(
                    input,
                    formType,
                    familyId: family.Id,
                    componentId: null,
                    ct);

                if(parameterValue != null)
                {
                    _db.ParameterValues.Add(parameterValue);
                }
            }

            _logger.LogDebug(
               "Добавлено {Count} параметров семейства в контекст",
               parameters.Count);
        }

        /// <summary>
        /// Сохранить параметры компонента
        /// </summary>
        private async Task SaveComponentParametersAsync(
            Component component,
            IReadOnlyList<ParameterValueInput> parameters,
            FormType formType,
            CancellationToken ct)
        {
            if (parameters == null || parameters.Count == 0)
            {
                _logger.LogDebug("Параметры компонента отсутствуют, пропускаем сохранение");
                return;
            }

            _logger.LogInformation(
                "Сохранение {Count} параметров для компонента Id={ComponentId}",
                parameters.Count,
                component.Id);

            // Создаём ParameterValue для каждого входного параметра
            foreach (var input in parameters)
            {
                var parameterValue = await CreateParameterValueAsync(
                    input,
                    formType,
                    familyId: null,
                    componentId: component.Id,
                    ct);

                if (parameterValue != null)
                {
                    _db.ParameterValues.Add(parameterValue);
                }
            }

            _logger.LogDebug(
              "Добавлено {Count} параметров компонента в контекст",
              parameters.Count);
        }

        /// <summary>
        /// Создать ParameterValue из входных данных
        /// </summary>
        private async Task<ParameterValue?> CreateParameterValueAsync(
           ParameterValueInput input,
           FormType formType,
           int? familyId,
           int? componentId,
           CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(input.ParameterCode))
            {
                _logger.LogWarning("Пропуск параметра с пустым кодом");
                return null;
            }

            // Ищем определение параметра (ParameterDefinition) по коду
            var parameterDefinition = formType.Parameters
                .FirstOrDefault(p => p.Code == input.ParameterCode);

            if (parameterDefinition == null)
            {
                _logger.LogWarning(
                    "ParameterDefinition с кодом '{Code}' не найден в FormType '{FormTypeCode}'. Пропускаем.",
                    input.ParameterCode,
                    formType.Code);

                // Варианты:
                // 1. Выбросить ошибку (строгая валидация)
                // 2. Пропустить параметр (мягкая валидация) ← используем этот подход
                // 3. Создать новый ParameterDefinition (автоматическое добавление)

                return null;
            }

            // Создаём ParameterValue
            var parameterValue = new ParameterValue
            {
                ParameterDefinitionId = parameterDefinition.Id,
                ComponentFamilyId = familyId,
                ComponentId = componentId,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // Сохраняем значение в зависимости от типа
            SetParameterValue(parameterValue, input.Value, parameterDefinition.ValueKind);

            // Если есть Pins (для WithPins типа)
            if (!string.IsNullOrWhiteSpace(input.Pins))
            {
                parameterValue.Pins = input.Pins;
            }

            _logger.LogDebug(
               "Создан ParameterValue: DefinitionId={DefinitionId}, Code={Code}, Value={Value}",
               parameterDefinition.Id,
               parameterDefinition.Code,
               input.Value);

            return parameterValue;
        }

        /// <summary>
        /// Установить значение параметра в зависимости от его типа
        /// </summary>
        private void SetParameterValue(
            ParameterValue parameterValue,
            string? value,
            ParameterValueKind valueKind)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // Пустое значение - сохраняем как null
                parameterValue.StringValue = null;
                parameterValue.DoubleValue = null;
                parameterValue.IntValue = null;
                return;
            }

            switch (valueKind)
            {
                case ParameterValueKind.String:
                case ParameterValueKind.WithPins:
                    parameterValue.StringValue = value;
                    break;

                case ParameterValueKind.Double:
                    if (double.TryParse(value, out var doubleVal))
                    {
                        parameterValue.DoubleValue = doubleVal;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Не удалось преобразовать '{Value}' в Double. Сохраняем как строку.",
                            value);
                        parameterValue.StringValue = value;
                    }
                    break;

                case ParameterValueKind.Int:
                    if (int.TryParse(value, out var intVal))
                    {
                        parameterValue.IntValue = intVal;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Не удалось преобразовать '{Value}' в Int. Сохраняем как строку.",
                            value);
                        parameterValue.StringValue = value;
                    }
                    break;

                default:
                    // Неизвестный тип - сохраняем как строку
                    parameterValue.StringValue = value;
                    _logger.LogWarning(
                        "Неизвестный ParameterValueKind: {ValueKind}. Сохраняем как строку.",
                        valueKind);
                    break;
            }
        }

        #endregion

    }
}
