using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Domain
{
    /// <summary>
    /// Result of importing a source file:
    /// 1. Document metadata from header
    /// 2. List of parsed components from the file
    /// 3. Resolved forms (UI/Word) for each component
    /// </summary>
    public class FileImportResultDto
    {

        public DocumentMetadataDto Metadata { get; set; } = new();

        /// <summary>
        /// Components as they appear in the source file.
        /// </summary>
        public IReadOnlyList<SourceComponentDto> Components { get; set; } = [];

        /// <summary>
        /// Resolved parameter forms from the database for each component.
        /// </summary>
        public IReadOnlyList<ComponentFormResultDto> ComponentForms { get; set; } = [];
    }
}
