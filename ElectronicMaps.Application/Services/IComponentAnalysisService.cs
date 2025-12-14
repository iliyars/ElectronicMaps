using ElectronicMaps.Application.WorkspaceProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public interface IComponentAnalysisService
    {
        Task<IReadOnlyList<ImportedRow>> AnalyzeAsync(
            Stream stream, CancellationToken ct = default);
    }
}
