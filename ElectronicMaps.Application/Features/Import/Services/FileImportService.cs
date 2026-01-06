

using ElectronicMaps.Application.Abstractions.Services;
using ElectronicMaps.Application.DTOs.Domain;

namespace ElectronicMaps.Application.Features.Import.Services
{
    /// <summary>
    /// Application layer service responsible for:
    /// 1. reading the source file,
    /// 2. extracting metadata + component list,
    /// 3. resolving component parameter forms from the DB.
    /// </summary>
    public class FileImportService : IFileImportService
    {
        private readonly IComponentSourceReader _reader;
        private readonly IComponentFormBatchService _batchService;

        public FileImportService(
            IComponentSourceReader reader,
            IComponentFormBatchService batchService)
        {
            _reader = reader;
            _batchService = batchService;
        }

        
        public async Task<FileImportResultDto> ImportAsync(string filePath, CancellationToken ct = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            // --- 1. Read the XML/CSV/etc file ------------------------
            using var stream = File.OpenRead(filePath);

            ComponentSourceFileDto src = await _reader.ReadAsync(stream, ct);

            // --- 2. Resolve parameter forms for each component --------
            var componentNames = src.Components
                .Select(c => c.CleanName)
                .ToList();

            var forms = await _batchService.BuildFormsAsync(componentNames, ct);

            // --- 3. Build the final result ----------------------------
            return new FileImportResultDto
            {
                Metadata = src.Metadata,
                Components = src.Components,
                ComponentForms = forms
            };
        }
    }
}
