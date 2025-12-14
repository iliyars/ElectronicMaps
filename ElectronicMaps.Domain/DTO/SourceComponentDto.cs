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
    public record SourceComponentDto
    (
        string RawName,
        string CleanName,
        string Type,
        string Family,

        int Quantity,
        string? Designators
    );
}
