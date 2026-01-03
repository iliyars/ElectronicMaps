using DocumentFormat.OpenXml.Packaging;
using ElectronicMaps.Documents.Core.Models;
using ElectronicMaps.Documents.Core.Services;
using ElectronicMaps.Documents.Rendering.FormRenderer;
using ElectronicMaps.Documents.Rendering.Schemas.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Rendering.OpenXml
{
    public class OpenXmlWordRenderer : IWordRenderer
    {
        private readonly ITemplateStore _templateStore;
        private readonly JsonSchemaStore _schemasStore;
        private readonly GenericFormRenderer _formRenderer;
        private readonly TableCloner _tableCloner;

        public OpenXmlWordRenderer(
            ITemplateStore templateStore,
            JsonSchemaStore schemasStore,
            GenericFormRenderer formRenderer,
            TableCloner tableCloner)
        {
            _templateStore = templateStore;
            _schemasStore = schemasStore;
            _formRenderer = formRenderer;
            _tableCloner = tableCloner;
        }

        public async Task<DocumentBuildResult> BuildAsync(DocumentBuildRequest request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var issues = new List<DocumentIssue>();

            try
            {
                // Загружаем шаблон
                await using var templateStream = _templateStore.OpenTemplate(request.TemplateId);
                await using var mem = new MemoryStream();
                await templateStream.CopyToAsync(mem, ct);
                mem.Position = 0;

                using (var doc = WordprocessingDocument.Open(mem, true))
                {
                    var mainPart = doc.MainDocumentPart;
                    if (mainPart?.Document?.Body == null)
                    {
                        issues.Add(new DocumentIssue(
                            DocumentIssueSeverity.Error,
                            "INVALID_TEMPLATE",
                            "Template document has no body"));
                        return new DocumentBuildResult(Array.Empty<byte>(), issues);
                    }

                    var schema = _schemasStore.GetSchema(request.TemplateId.FormCode);

                    // Разбиваем элементы по страницам
                    var pages = Chunk(request.Items, schema.ItemsPerPage);

                    if (pages.Count == 0)
                    {
                        issues.Add(new DocumentIssue(
                            DocumentIssueSeverity.Warning,
                            "NO_ITEMS",
                            "No items to render"));

                        // Заполняем шаблон пустыми значениями
                        _formRenderer.RenderForm(mainPart.Document.Body, schema, new List<DocumentItem>());
                    }
                    else if (pages.Count == 1)
                    {
                        // Одна страница - просто заполняем шаблон
                        _formRenderer.RenderForm(mainPart.Document.Body, schema, pages[0]);
                    }
                    else
                    {
                        // Множество страниц - клонируем таблицу
                        try
                        {
                            // Клонируем таблицу-шаблон
                            var tables = _tableCloner.CloneTemplateTable(
                                mainPart.Document.Body,
                                "TemplateTable",
                                pages.Count);

                            // Заполняем каждую таблицу своими данными
                            for (int i = 0; i < pages.Count && i < tables.Count; i++)
                            {
                                _formRenderer.RenderForm(tables[i], schema, pages[i]);
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            issues.Add(new DocumentIssue(
                                DocumentIssueSeverity.Error,
                                "TEMPLATE_ERROR",
                                $"Failed to clone template table: {ex.Message}"));

                            // Fallback: заполняем только первую страницу
                            _formRenderer.RenderForm(mainPart.Document.Body, schema, pages[0]);

                            issues.Add(new DocumentIssue(
                                DocumentIssueSeverity.Warning,
                                "PARTIAL_RENDER",
                                $"Only first page rendered. Total pages: {pages.Count}"));
                        }
                    }

                    // Заполняем мета-поля (если есть)
                    if (schema.MetaFieldTags != null)
                    {
                        var writer = new SdtContentWriter();
                        foreach (var (key, tag) in schema.MetaFieldTags)
                        {
                            // Здесь можно добавить логику для заполнения мета-полей
                            // Например, номер документа, дата и т.д.
                            // writer.TryWrite(mainPart.Document.Body, tag, metaValue);
                        }
                    }

                    mainPart.Document.Save();
                }

                mem.Position = 0;
                return new DocumentBuildResult(mem.ToArray(), issues);
            }
            catch (Exception ex)
            {
                issues.Add(new DocumentIssue(
                    DocumentIssueSeverity.Error,
                    "BUILD_FAILED",
                    $"Document build failed: {ex.Message}"));

                return new DocumentBuildResult(Array.Empty<byte>(), issues);
            }
        }

        /// <summary>
        /// Разбивает список на чанки (страницы)
        /// </summary>
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