using ElectronicMaps.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Stores
{
    public interface IComponentStoreSerializer
    {
        /// <summary>Сериализует набор компонентов в поток.</summary>
        Task SerializeAsync(
            Stream stream,
            IReadOnlyDictionary<string, List<AnalyzedComponentDto>> map,
            CancellationToken ct = default);

        /// <summary>Десериализует набор компонентов из потока.</summary>
        Task<Dictionary<string, List<AnalyzedComponentDto>>> DeserializeAsync(
            Stream stream,
            CancellationToken ct = default);
    }
}
