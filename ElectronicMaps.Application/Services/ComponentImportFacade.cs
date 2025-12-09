using ElectronicMaps.Application.DTO;
using ElectronicMaps.Application.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public class ComponentImportFacade : IComponentImportFacade
    {
        private readonly IComponentAnalysisService _analysis;
        private readonly IComponentStore _store;

        public ComponentImportFacade(IComponentAnalysisService analysis, IComponentStore store)
        {
            _analysis = analysis;
            _store = store;
        }


        public async Task<IReadOnlyList<AnalyzedComponentDto>> AnalyzeAndStoreAsync(Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            var analyzed = await _analysis.AnalyzeAsync(stream, ct);

            _store.ReplaceAll(analyzed);
            return analyzed;
        }
    }
}
