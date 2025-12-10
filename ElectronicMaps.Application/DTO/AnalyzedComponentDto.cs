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
        public string Type { get; init; } = default!;         // "Резистор", "Конденсатор", "Микросхема"...
        public string Family { get; init; } = default!;       // "К10-79", "ОСМ 1594ТЛ2Т" и т.п.
        public string CleanName { get; init; } = default!;         // полное наименование без ТУ
        public string CanonicalName { get; init; } = default!; // нормализованный ключ
        public int Quantity { get; init; }
        public string? Designators { get; set; }

        // результат анализа/резолва
        public string ResolvedFamily { get; init; } = default!;      // нормализованное семейство
        public string? ResolvedFormCode { get; init; }               // форма компонента ИЛИ null
        public bool FormResolved => ResolvedFormCode is not null;    // удобный флаг



        // связь с БД
        public bool ExistsInDatabase { get; init; }
        public int? ExistingComponentId { get; init; }
        public string? DatabaseName { get; init; }
        public string? ComponentFormCode { get; init; }
        public int? DatabaseFamilyId { get; init; }
        public string? DatabaseFamilyFormCode { get; init; }
        public bool FamilyExistsInDatabase { get; init; }



        public IReadOnlyList<ParameterDto> Parameters { get; set; } = Array.Empty<ParameterDto>();

        public IReadOnlyList<ParameterDto> SchematicParameters { get; set; } = Array.Empty<ParameterDto>();

        public IReadOnlyList<ParameterDto> FamilyParameters { get; init; } = Array.Empty<ParameterDto>();
        public IReadOnlyList<ParameterDto> FamilySchematickParameters { get; init; } = Array.Empty<ParameterDto>();

        public bool IsEdited { get; set; } = false;
        public bool IsDirty { get; set; } = false;
        public bool IsSelectedForImport { get; set; } = false;

        public DateTimeOffset LastUpdatedUtc { get; set; } = DateTimeOffset.UtcNow;

    }
}
