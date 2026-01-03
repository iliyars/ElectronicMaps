using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Core.Models
{
    public sealed record DocumentBuildRequest(
        TemplateId TemplateId,
        IReadOnlyList<DocumentItem> Items,
        DocumentBuildOptions? Options = null
        );


    public sealed record DocumentBuildOptions(
        string? TemplateRoot = null,
        bool ValidateTemplate = true);  
}
