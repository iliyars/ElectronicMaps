using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ElectronicMaps.Infrastructure.Services
{
    /// <summary>
    /// Reads AVS XML (.PE.XML) fiels and extracts:
    /// 1) Document metadata (PassportData)
    /// 2) Component list (RecordsData)
    /// This reader extracts records from <RecordsData></RecordsData> sectrion.
    /// </summary>
    public class AvsXmlComponentSourceReader : IComponentSourceReader
    {

        private readonly IComponentNameParser _nameParser;

        public AvsXmlComponentSourceReader(IComponentNameParser nameParser)
        {
            _nameParser = nameParser;
        }

        
        /// <inheritdoc/>
        public async Task<ComponentSourceFileDto> ReadAsync(Stream source, CancellationToken ct = default)
        {
            var result = new ComponentSourceFileDto();
            var components = new List<SourceComponentDto>();

            var settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using var reader = XmlReader.Create(source, settings);

            string? currentSection = null;

            while (await reader.ReadAsync())
            {
                ct.ThrowIfCancellationRequested();

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "PassportData")
                        currentSection = "passport";

                    if(reader.Name == "RecordsData")
                        currentSection = "records";

                    if(reader.Name == "Record")
                    {
                        if (currentSection == "passport")
                            ReadPassportRecord(reader, result.Metadata);

                        if(currentSection == "records")
                            ReadComponentRecord(reader, components);
                    }
                }
            }

            result.Components = components;

            return result;
        }

        private void ReadPassportRecord(XmlReader reader, DocumentMetadataDto metadata)
        {
            // We take only the fields we need
            metadata.Designation ??= reader.GetAttribute("field_1");
            metadata.Title ??= reader.GetAttribute("field_2");
            metadata.Developer ??= reader.GetAttribute("field_9");
            metadata.Topic ??= reader.GetAttribute("field_63");
            metadata.DocumentNumber ??= reader.GetAttribute("field_137");
        }

        private void ReadComponentRecord(XmlReader reader, List<SourceComponentDto> list)
        {
            // Skip group headers and notes
            var typeRec = reader.GetAttribute("type_rec");
            if (typeRec == "X" || typeRec == "R")
                return;

            var name = reader.GetAttribute("field_5");
            if(string.IsNullOrWhiteSpace(name))
                return;

            // 2. Признак платы/изделия по структуре:
            // присутствуют служебные поля field_10 / field_14
            var hasAssemblyMarkers =
                reader.GetAttribute("field_10") != null ||
                reader.GetAttribute("field_14") != null;
            if (hasAssemblyMarkers)
                return;

            int.TryParse(reader.GetAttribute("field_6"), out int qty);
            var design = reader.GetAttribute("field_32");


            var parsed = _nameParser.Parse(name);

            list.Add(new SourceComponentDto(
                RawName: name,
                Type: parsed.Type,
                Family: parsed.Family,
                CleanName: parsed.Name,
                Quantity: qty,
                Designators: design?.Trim()
            ));
        }
    }
}
