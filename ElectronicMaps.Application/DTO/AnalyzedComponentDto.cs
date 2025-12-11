using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO
{
    public class AnalyzedComponentDto
    {
        public string RawName { get; init; } = default!; // строка из XML
        public string CleanName { get; init; } = default!;         // полное наименование без ТУ
        public string? Family { get; init; } = default!;       // "К10-79", "ОСМ 1594ТЛ2Т" и т.п.
        public string Type { get; init; } = default!;         // "Резистор", "Конденсатор", "Микросхема"...
        public int Quantity { get; init; }
        public string? Designator { get; set; }
        public IReadOnlyList<string> Designators { get; init; } = Array.Empty<string>();



        // связь с БД
        public bool ComponentExistsInDatabase { get; init; }
        public int? ExistingComponentId { get; init; }
        public string? DatabaseComponentName { get; init; }

        public bool FamilyExistsInDatabase { get; init; }
        public int? DatabaseFamilyId { get; init; }
        public string? DatabaseFamilyName { get; init; }

        // Информация о формах
        // Форма для семейства
        public int? FamilyFormTypeId { get; init; }
        public string? FamilyFormTypeCode { get; init; }
        public string? FamilyFormDisplayName { get; init; }

        // Форма для компонента
        public int? ComponentFormTypeId { get; init; }
        public string? ComponentFormTypeCode { get; init; }
        public string? ComponentFormDisplayName { get; init; }

        //Параметры
        public IReadOnlyList<ParameterDto> FamilyParameters { get; init; } = Array.Empty<ParameterDto>();
        public IReadOnlyList<ParameterDto> ComponentParameters { get; init; } = Array.Empty<ParameterDto>();
        public IReadOnlyList<ParameterDto> SchematicComponentParameters { get; init; } = Array.Empty<ParameterDto>();

        // Для UI
        public bool IsEdited { get; set; } = false;
        public bool IsDirty { get; set; } = false;
        public bool IsSelectedForImport { get; set; } = false;

        public DateTimeOffset LastUpdatedUtc { get; set; } = DateTimeOffset.UtcNow;

    }
}
