using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Word.DrawingCanvas;
using DocumentFormat.OpenXml.Packaging;
using ElectronicMaps.Documents.Abstractions.Models;
using ElectronicMaps.Documents.Abstractions.Services;
using ElectronicMaps.Documents.OpenXmlWord.Forms;
using ElectronicMaps.Documents.OpenXmlWord.Forms.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.OpenXmlWord.Engine
{
    public class OpenXmlWordRenderer : IWordRender
    {
        private readonly ITemplateStore _templateStore;
        private readonly JsonSchemaStore _schemasStore;
        private readonly GenericSdtFormRenderer _formRenderer;

        public OpenXmlWordRenderer(ITemplateStore templateStore, JsonSchemaStore schemasStore, GenericSdtFormRenderer formRenderer)
        {
            _templateStore = templateStore;
            _schemasStore = schemasStore;
            _formRenderer = formRenderer;
        }

        public async Task<DocumentBuildResult> BuildAsync(DocumentBuildRequest request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var issues = new List<DocumentIssue>();

            await using var templateStream = _templateStore.OpenTemplate(request.TemplateId);
            await using var mem = new MemoryStream();
            await templateStream.CopyToAsync(mem, ct);
            mem.Position = 0;

            using (var doc = WordprocessingDocument.Open(mem, true))
            {
                var mainPart = doc.MainDocumentPart;

                var schema = _schemasStore.Get(request.TemplateId.FormCode);

                //Разбиваем страницы по ItemsPerPage
                var pages = Chunk(request.Items, schema.ItemsPerPage);

                //Заполняем первую страниицу
                var firstPageItems = pages.FirstOrDefault() ?? new List<DocumentItem>();
                _formRenderer.RenderForm(smainPart, schema, firstPageItems);

                mainPart.Document.Save();
            }

            return new DocumentBuildResult(mem.ToArray(), issues);
        }

        private static List<List<T>> Chunk<T>(IReadOnlyList<T> items, int size)
        {
            var result = new List<List<T>>();
            for (int i = 0; i < items.Count; i += size)
            {
                var chunk = items.Skip(i).Take(size).ToList();
                result.Add(chunk);
            }
            return result;
        }
    }
}
