using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Services
{
    public interface IComponentSourceReader
    {
        /// <summary>
        /// Reads a list of component from an external source (file, stream, etc.).
        /// Implementation may  support different formats (XML, Excel, CSV, etc.).
        /// </summary>
        /// <param name="source">
        /// Input stream containing the component list data.'
        /// The caller is responsble for opening and disposing the stram.</param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <returns>Passport data and read-only list of components as they appear in the source.</returns>
        Task<ComponentSourceFileDto> ReadAsync(
            Stream source,
            CancellationToken ct = default);


    }
}
