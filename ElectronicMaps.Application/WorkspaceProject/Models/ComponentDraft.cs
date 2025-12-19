using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.WorkspaceProject.Models
{
    public enum DraftKind
    {
        Component = 0,
        FamilyAgregate = 1
    }
    public enum LocalFillStatus { Missing, Filled }
    public enum ApprovalStatus
    {
        Missing = 0,
        Draft = 1,
        Pending = 2,
        Approved = 3,
        Rejected = 4
    }

    public record ApprovalRef(
        ApprovalStatus Status,
        Guid? PendingSetId,
        Guid? ApprovedSetId,
        string? RejectReason = null
    );


    public record ComponentDraft(
    Guid Id,
    Guid SourceRowId,  // Откуда он произошёл (строка импорта)

    string Name,
    int? DBComponentId,

    string? Family,
    int? DbFamilyId,

    int? DbComponentFormId,
    string FormCode,
    string FormName,

    int Quantity,

    DraftKind Kind,


    
    IReadOnlyList<string> Designators,  // Обозначения (R1–R5 и т.п.)

    IReadOnlyList<int> SelectedRemarksIds,

    IReadOnlyDictionary<int, ParameterValueDraft> NdtParametersOverrides,   // локальный переобределения параметров по НДТ
    ApprovalRef ApprovalRef,

    IReadOnlyDictionary<int, ParameterValueDraft> SchematicParameters,
    LocalFillStatus LocalFillStatus
        );

}
