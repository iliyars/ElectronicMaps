using ElectronicMaps.Documents.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Abstractions.Services
{
    public interface IWordRender
    {
        Task<DocumentBuildResult> BuildAsync(DocumentBuildRequest request, CancellationToken ct);
    }
}
