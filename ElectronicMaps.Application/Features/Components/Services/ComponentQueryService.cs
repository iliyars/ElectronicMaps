using ElectronicMaps.Application.Abstractions.Queries.Forms;
using ElectronicMaps.Application.Abstractions.Queries.Parameters;
using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Application.Features.Workspace.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Components.Services
{
    // Сервис для получеиня данных о компонентах и формах (для UI)
    public class ComponentQueryService : IComponentQueryService
    {
        private readonly IFormTypeReadRepository _formTypes;
        private readonly IParameterDefinitionReadRepository _parameterDefinitions;
        private readonly ILogger<ComponentQueryService> _logger;

        private const string FamilyFormTypeCode = WorkspaceViewKeys.FamilyFormCode;

        public ComponentQueryService(
        IFormTypeReadRepository formTypes,
        IParameterDefinitionReadRepository parameterDefinitions,
        ILogger<ComponentQueryService> logger)
        {
            _formTypes = formTypes ?? throw new ArgumentNullException(nameof(formTypes));
            _parameterDefinitions = parameterDefinitions ?? throw new ArgumentNullException(nameof(parameterDefinitions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Получение параметров семейства для заполнения
        /// </summary>
        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetFamilyParameterDefinitionsAsync(CancellationToken ct = default)
        {
            _logger.LogDebug("Получение параметров семейства для FormType: {FormTypeCode}", FamilyFormTypeCode);

            try
            {
                var parameters = await _parameterDefinitions.GetByFormCodeAsync(FamilyFormTypeCode, ct);

                _logger.LogInformation(
                    "Получено {Count} параметров семейства", parameters.Count);

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
        /// <summary>
        /// Получить список доступных форм компонента
        /// </summary>
        public async Task<IReadOnlyList<FormTypeDto>> GetAvailableComponentFormsAsync(CancellationToken ct = default)
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

        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetComponentParameterDefinitionsAsync(
            string formTypeCode,
            CancellationToken ct = default)
        {
            if(string.IsNullOrWhiteSpace(formTypeCode))
            {
                throw new ArgumentException("Код формы не может быть пустым", nameof(formTypeCode));
            }

            _logger.LogDebug(
                "Получение параметров компонента для FormType: {FormTypeCode}",
                formTypeCode);

            try
            {
                var componentFormType = await _formTypes.GetByCodeAsync(formTypeCode, ct);
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
    }
}
