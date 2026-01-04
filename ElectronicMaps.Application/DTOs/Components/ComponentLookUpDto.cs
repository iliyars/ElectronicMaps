using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Components
{
    public record ComponentLookUpDto(
        int Id,
        string Name,

        int FormTypeId,
        string FormCode,
        string FormName,

        int ComponentFamilyId,
        string FamilyName,
        VerificationStatus VerificationStatus
        );

}
