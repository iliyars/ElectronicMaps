using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Features.Workspace.Models
{
    public record ParameterValueDraft(
         int ParameterDefinitionId,

    string Code,
    string DisplayName,
    string? Unit,

    string? StringValue,
    double? DoubleValue,
    int? IntValue,
    string? Pins
        );


}
