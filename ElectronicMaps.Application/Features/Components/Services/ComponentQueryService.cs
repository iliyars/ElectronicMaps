using ElectronicMaps.Application.Abstractions.Queries.Forms;
using ElectronicMaps.Application.Abstractions.Queries.Parameters;
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
        // Получение параметров семейства для заполнения
        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetFamilyParameterDefinitionsAsync(CancellationToken ct)
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

    }
}
