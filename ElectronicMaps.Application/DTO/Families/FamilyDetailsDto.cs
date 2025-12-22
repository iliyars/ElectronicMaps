using ElectronicMaps.Application.DTO.Forms;
using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO.Families
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
