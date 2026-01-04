using ElectronicMaps.Application.Features.Workspace.Models;
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
            IReadOnlyDictionary<string, List<ImportedRow>> map,
            CancellationToken ct = default);

        /// <summary>Десериализует набор компонентов из потока.</summary>
        Task<Dictionary<string, List<ImportedRow>>> DeserializeAsync(
            Stream stream,
            CancellationToken ct = default);
    }
}
