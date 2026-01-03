using ElectronicMaps.Documents.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Services
{
    public interface IDocumentGenerationService
    {
        Task<DocumentBuildResult> GenerateDocumentAsync(
            int formTypeId,
            IEnumerable<int> componentIds,
            DocumentBuildOptions? options = null,
            CancellationToken ct = default);

        Task SaveDocumentAsync(
            DocumentBuildResult result,
            string filePath,
            CancellationToken ct = default);
    }
}
