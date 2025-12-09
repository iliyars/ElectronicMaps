using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.DTO
{
    public class ParameterDto
    {
        public string Code { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public string? Unit { get; init; }
        public string? StringValue { get; init; }
        public double? DoubleValue { get; init; }
        public int? IntValue { get; init; }
        public string? Pins { get; init; }
    }
}
