using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Components
{
    public record SaveComponentRequest(
        string ComponentName,
        string ComponentFormTypeCode,

        int? ExistingFamilyId,
        string FamilyName,


        IReadOnlyList<ParameterValueInput> ComponentParameters,
        IReadOnlyList<ParameterValueInput> FamilyParameters
        );
}
