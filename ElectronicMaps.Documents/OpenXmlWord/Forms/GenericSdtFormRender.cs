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
    public class GenericSdtFormRender
    {
        private readonly SdtFileWriter _writer;

        public GenericSdtFormRender(SdtFileWriter writer)
        {
            _writer = writer;
        }

        public int RenderForm(MainDocumentPart mainPart, FormSchema schema, IReadOnlyList<DocumentItem> itemsOnPage)
        {
            // itemsOnPage.Count <= schema.ItemsPerPage
            for(int index = 1; index <= schema.ItemsPerPage; index++)
            {
                var item = (index - 1 < itemsOnPage.Count) ? itemsOnPage[index-1] : null;
            }
        }


    }
}
