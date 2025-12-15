using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.WorkspaceProject.Models
{
    public record ComponentDraft(
    Guid Id,

    // Откуда он произошёл (строка импорта)
    Guid SourceRowId,

    // Отображаемое имя
    string Name,
    int? DBComponentId,
    // Семейство
    string? Family,
    int? DbFamilyId,
    // Форма, по которой он сейчас классифицирован
    string FormCode,
    int Quantity,
    //string Designator,
    // Обозначения (R1–R5 и т.п.)
    IReadOnlyList<string> Designators,

    IReadOnlyList<int> SelectedRemarksIds,

    // Параметры (ключ — ParameterDefinitionId)
    IReadOnlyDictionary<int, ParameterValueDraft> NdtParametersOverrides,

    IReadOnlyDictionary<int, ParameterValueDraft> SchematicParameters
        );

}
