using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.DTO
{
    /// <summary>
    /// Represents a single component entry loaded from an external source. (XML, Excel ...)
    /// </summary>
    public class SourceComponentDto
    {
        public string RawName { get; set; } = default!;
        public string CleanName { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Family { get; set; } = default!;

        public int Quantity { get; set; }
        public string? Designators { get; set; }


    }
}
