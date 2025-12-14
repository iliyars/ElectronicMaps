using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO.Parameters
{
    public record ParameterValueDto(
        int ParameterDefinitionId,
        string Code,
        string? StringValue,
        double? DoubleValue,
        int? IntValue,
        string pins
        );

}
