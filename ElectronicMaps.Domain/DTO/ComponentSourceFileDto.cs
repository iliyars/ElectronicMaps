using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.DTO
{
    public class ComponentSourceFileDto
    {
        public DocumentMetadataDto Metadata { get; set; } = new();
        public IReadOnlyList<SourceComponentDto> Components { get; set; } = [];


    }
}
