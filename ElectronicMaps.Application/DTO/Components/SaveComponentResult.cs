using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO.Components
{
    public record SaveComponentResult(
       int ComponentId,
       VerificationStatus ComponentVerificationStatus,

       int ComponentFamilyId,
       VerificationStatus FamilyVerificationStatus,

       bool ComponentWasCreated,
       bool FamilyWasCreated
    );
}
