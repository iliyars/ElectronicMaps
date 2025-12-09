using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    /// <summary>
    /// High-level application service that loads a source file
    /// (XML, CSV, etc.) and produces parsed document metadata and
    /// parameter forms for all listed components.
    /// </summary>
    public interface IFileImportService
    {
        /// <summary>
        /// Imports a file containing a component list and returns parsed
        /// document metadata along with resolved component parameter forms.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<FileImportResultDto> ImportAsync(
            string filePath,
            CancellationToken ct = default);


    }
}
