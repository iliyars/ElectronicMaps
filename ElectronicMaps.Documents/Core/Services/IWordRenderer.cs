using ElectronicMaps.Documents.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Core.Services
{
    public interface IWordRenderer
    {
        Task<DocumentBuildResult> BuildAsync(DocumentBuildRequest request, CancellationToken ct);
    }
}
