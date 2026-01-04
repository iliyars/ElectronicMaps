using ElectronicMaps.Application.Features.Workspace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Features.Components.Services
{
    public interface IComponentAnalysisService
    {
        Task<IReadOnlyList<ImportedRow>> AnalyzeAsync(
            Stream stream, CancellationToken ct = default);
    }
}
