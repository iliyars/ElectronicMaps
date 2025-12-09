using ElectronicMaps.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public interface IComponentImportFacade
    {
       Task<IReadOnlyList<AnalyzedComponentDto>> AnalyzeAndStoreAsync(
       Stream stream,
       bool overwrite = false,
       CancellationToken ct = default);
    }
}
