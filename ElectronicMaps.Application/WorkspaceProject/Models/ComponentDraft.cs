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

    // Семейство
    string? Family,

    // Форма, по которой он сейчас классифицирован
    string FormCode,

    // Обозначения (R1–R5 и т.п.)
    IReadOnlyList<string> Designators,

    // Параметры (ключ — ParameterDefinitionId)
    IReadOnlyDictionary<int, ParameterValueDraft> Parameters
        );

}
