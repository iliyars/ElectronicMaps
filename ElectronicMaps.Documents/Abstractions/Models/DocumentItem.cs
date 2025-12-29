using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Abstractions.Models
{
    public sealed record DocumentItem(
        string Name, 
        string? Designators,
        int? Quantity,
        IReadOnlyDictionary<string, string?> Fields);
}
