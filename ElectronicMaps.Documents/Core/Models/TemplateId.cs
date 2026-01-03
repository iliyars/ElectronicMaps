using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Core.Models
{
    public sealed record TemplateId(string FormCode, string? Version = null);
}
