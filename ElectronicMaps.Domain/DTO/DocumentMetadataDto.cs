using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.DTO
{
    /// <summary>
    /// Represents metadata extracted from the document header section of AVS XML.
    /// </summary>
    public class DocumentMetadataDto
    {
        public string? Designation { get; set; }      // field_1
        public string? Title { get; set; }            // field_2
        public string? Developer { get; set; }        // field_9
        public string? Topic { get; set; }            // field_63
        public string? DocumentNumber { get; set; }   // field_137



    }
}
