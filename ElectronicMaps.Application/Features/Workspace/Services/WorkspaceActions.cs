using ElectronicMaps.Application.Abstractions.Queries.Parameters;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Workspace.Services
{
    public class WorkspaceActions : IWorkspaceActions
    {
        private readonly IComponentStore _store;
        private readonly IParameterDefinitionReadRepository _parameterDefinitions;
        private readonly ILogger<WorkspaceActions> _logger;

        public WorkspaceActions(IComponentStore store, IParameterDefinitionReadRepository parameterDefinitionReadRepository, ILogger<WorkspaceActions> logger)
        {
            _store = store;
            _parameterDefinitions = parameterDefinitionReadRepository;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ParameterDefinitionDto>> LoadDefinitionsForDraftAsync(Guid draftId, CancellationToken ct)
        {
            var draft = _store.TryGetWorking(draftId);

            if(draft == null)
            {
                _logger.LogWarning("LoadDefinitionsForDraftAsync: draft {DraftId} не найден в store", draftId);
                return Array.Empty<ParameterDefinitionDto>();
            }

            // Для неопределённых компонентов параметров нет
            if (string.IsNullOrWhiteSpace(draft.FormCode) ||
                string.Equals(draft.FormCode, WorkspaceViewKeys.UndefinedForm, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Draft '{Name}' имеет неопределённую форму — параметры не загружаются", draft.Name);
                return Array.Empty<ParameterDefinitionDto>();
            }

            _logger.LogDebug("Загрузка определений параметров для '{Name}' (FormCode: {FormCode})",
                draft.Name, draft.FormCode);

            var definitions = await _parameterDefinitions.GetByFormCodeAsync(draft.FormCode, ct);

            _logger.LogInformation("Загружено {Count} определений для формы '{FormCode}'",
                definitions.Count, draft.FormCode);

            return definitions;
        }

        public Task SaveDraftToDatabaseAsync(Guid draftId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
