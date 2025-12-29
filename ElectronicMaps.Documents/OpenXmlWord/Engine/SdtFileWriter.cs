using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.OpenXmlWord.Engine
{
    public class SdtFileWriter
    {
        public bool TryWrite(MainDocumentPart mainPart, string tag, string? value)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;

            // Ищем все content controls (SDT) в документе по Tag
            var sdts = mainPart.Document.Descendants<SdtElement>()
                .Where(s => string.Equals(GetTag(s), tag, StringComparison.Ordinal))
                .ToList();

            if (sdts.Count == 0) return false;
        }


        private static string? GetTag(SdtElement sdt) => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value;


        private static void SetSdtText(SdtElement sdt, string text)
        {
            // Универсально: кладём текст в Run/Text внутри SDT.
            // Если там таблица/сложный контент — это можно расширить позже.
            var content = sdt.SdtContentBlock as OpenXmlCompositeElement
                          ?? sdt.SdtContentRun as OpenXmlCompositeElement;

            if (content is null) return;

            //Удаляем старый текст
            foreach (var t in content.Descendants<Text>().ToList())
                t.Remove();

            // Пишем новый
            var run = content.Descendants<Run>().FirstOrDefault();
            if(run is null)
            {
                run = new Run();
                content.AppendChild(run);
            }

            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

        }
    }
}
