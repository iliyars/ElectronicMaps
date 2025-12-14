using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.WorkspaceProject.Models
{
    public record ImportedRow
    (
        Guid RowId,

        // XML
        string RawName, // строка из XML
        string CleanName,         // полное наименование без ТУ
        string? Family,       // "К10-79", "ОСМ 1594ТЛ2Т" и т.п.
        string Type,        // "Резистор", "Конденсатор", "Микросхема"...
        int Quantity,
        string? Designator,
        IReadOnlyList<string> Designators,



        // связь с БД
        bool ComponentExistsInDatabase,
        int? ExistingComponentId,
        string? DatabaseComponentName,

        bool FamilyExistsInDatabase,
        int? DatabaseFamilyId,
        string? DatabaseFamilyName,

        // Информация о формах
        // Форма для семейства
        int? FamilyFormId,
        string? FamilyFormTypeCode,
        string? FamilyFormDisplayName,

        // Форма для компонента
        int? ComponentFormId,
        string? ComponentFormCode,
        string? ComponentFormDisplayName,

        DateTimeOffset LastUpdatedUtc
    );

}
