using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Parameters
{
    public record ParameterDefinitionDto(
        int Id,
        string Code,
        string DisplayName,
        ParameterValueKind DataType,     //string, int, float
        int Order,
        string? Unit
        //bool IsRequired,
        );

}
