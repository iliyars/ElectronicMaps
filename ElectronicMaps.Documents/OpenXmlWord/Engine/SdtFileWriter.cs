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
        public bool TryWrite(OpenXmlElement scope, string tag, string? value)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;

            // Ищем все content controls (SDT) в документе по Tag
            var sdts = scope.Descendants<SdtElement>()
                .Where(s => string.Equals(GetTag(s), tag, StringComparison.Ordinal))
                .ToList();

            if (sdts.Count == 0) return false;

            foreach (var sdt in sdts)
            {
                SetSdtText(sdt, value ?? string.Empty);
            }

            return true;
        }


        private static string? GetTag(SdtElement sdt) => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value;


        private static void SetSdtText(SdtElement sdt, string text)
        {
            if (sdt is SdtRun sdtRun)
            {
                SetTextInRunConent(sdtRun.SdtContentRun, text);
                return;
            }

            if (sdt is SdtBlock sdtBlock)
            {
                SetTextInBlockContent(sdtBlock.SdtContentBlock, text);
                return;
            }

            throw new NotSupportedException($"SDT type '{sdt.GetType().FullName}' is not supported for text setting.");
        }

        private static void SetTextInBlockContent(SdtContentBlock? content, string text)
        {
            if (content == null) return;

            // Для block-контента обычно есть Paragraph
            var para = content.Descendants<Paragraph>().FirstOrDefault();
            if (para == null)
            {
                para = new Paragraph(new Run());
                content.AppendChild(para);
            }

            foreach (var t in para.Descendants<Text>().ToList())
                t.Remove();

            var run = para.Descendants<Run>().FirstOrDefault();
            if (run == null)
            {
                run = new Run();
                para.AppendChild(run);
            }

            run.AppendChild(new Text(text)
            {
                Space = SpaceProcessingModeValues.Preserve
            });
        }

        private static void SetTextInRunConent(SdtContentRun? content, string text)
        {
            if (content == null) return;

            //Удаляем старый текст
            foreach(var t in content.Descendants<Text>().ToList())
            {
                t.Remove();
            }

            var run = content.Descendants<Run>().FirstOrDefault();
            if(run == null)
            {
                run = new Run();
                content.AppendChild(run);
            }

            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        }
    }

}
