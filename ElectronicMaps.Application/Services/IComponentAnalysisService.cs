using ElectronicMaps.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public interface IComponentAnalysisService
    {
        Task<IReadOnlyList<AnalyzedComponentDto>> AnalyzeAsync(
            Stream stream, CancellationToken ct = default);
    }
}
