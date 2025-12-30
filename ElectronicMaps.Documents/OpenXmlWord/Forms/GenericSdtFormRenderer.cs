using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using ElectronicMaps.Documents.Abstractions.Models;
using ElectronicMaps.Documents.OpenXmlWord.Engine;
using ElectronicMaps.Documents.OpenXmlWord.Forms.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.OpenXmlWord.Forms
{
    public class GenericSdtFormRenderer
    {
        private readonly SdtFileWriter _writer;

        public GenericSdtFormRenderer(SdtFileWriter writer)
        {
            _writer = writer;
        }

        public int RenderForm(OpenXmlElement scope, FormSchema schema, IReadOnlyList<DocumentItem> itemsOnPage)
        {
            // itemsOnPage.Count <= schema.ItemsPerPage
            for(int index = 1; index <= schema.ItemsPerPage; index++)
            {
                var item = (index - 1 < itemsOnPage.Count) ? itemsOnPage[index-1] : null;

                foreach (var (fieldKey, tagTemplate) in schema.ItemFieldTagTemplates)
                {
                    var tag = tagTemplate.Replace("{i}", index.ToString(), StringComparison.Ordinal);

                    string? value = item is null ? null : ResolvedFieldValue(item, fieldKey);

                    _writer.TryWrite(scope, tag, value);
                }
            }

            return schema.ItemsPerPage;
        }

        private static string? ResolvedFieldValue(DocumentItem item, string fieldKey)
        {
            return fieldKey switch
            {
                "Name" => item.Name,
                "Designators" => item.Designators,
                "Quantity" => item.Quantity?.ToString(),
                _ => item.Fields.TryGetValue(fieldKey, out var val) ? val : null,
            };
        }


    }
}
