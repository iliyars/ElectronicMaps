using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Families
{
    public record FamilyDetailsDto(
        int Id,
        string Name,
        FormTypeDto FormType,
        VerificationStatus VerificationStatus,
        int ComponentsCount,
        IReadOnlyList<ParameterDefinitionDto> ParameterDefinitions,
        IReadOnlyList<ParameterValueDto> ParameterValues
        );
}
