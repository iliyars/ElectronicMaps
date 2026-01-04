using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Parameters
{
    public record ParameterValueInput
    (
        int ParameterDefinitionId,

        string? StringValue,
        double? DoubleValue,
        int? IntValue,

        string? Pins
    );
}
