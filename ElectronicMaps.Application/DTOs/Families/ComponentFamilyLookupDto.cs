using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Families
{
    public record ComponentFamilyLookupDto(
        int Id,
        string Name,
        int FormTypeId,
        string FormCode,
        VerificationStatus VerificationStatus
        );

}
