using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO.Components
{
    public record SaveComponentRequest(
        string ComponentName,
        int ComponentFormTypeId,

        int? ExistingFamilyId,
        string FamilyName,

        int FamilyFormTypeId,

        IReadOnlyList<ParameterValueInput> ComponentParameters,
        IReadOnlyList<ParameterValueInput> FamilyParameters
        );
}
