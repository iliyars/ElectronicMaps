using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.ValueObjects
{
    public class ParsedComponentName
    {
        public string Raw { get; init; } = default!;
        public string Type { get; init; } = default!;
        public string Family { get; init; } = default!;
        public string Name { get; init; } = default!;
    }
}
