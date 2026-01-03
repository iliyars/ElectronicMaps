using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ElectronicMaps.Documents.Core.Models;
using ElectronicMaps.Documents.Core.Services;
using ElectronicMaps.Documents.Rendering.Schemas.Models;
using ElectronicMaps.Documents.Rendering.Schemas.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ElectronicMaps.Documents.Rendering.Validation
{
    public class TemplateValidator : ITemplateValidator
    {
        private readonly ITemplateStore _templateStore;
        private readonly ISchemaStore _schemaStore;

        public TemplateValidator(ITemplateStore templateStore, ISchemaStore schemaStore)
        {
            _templateStore = templateStore;
            _schemaStore = schemaStore;
        }


        public ValidationResult Validate(Stream templateStream, FormSchema schema)
        {
            var result = new ValidationResult();

            try
            {
                // Валидация схема
                try
                {
                    schema.Validate();
                }
                catch(Exception ex)
                {
                    result.AddError("SCHEMA_INVALID", $"Schema validation failed: {ex.Message}");
                    return result;
                }

                using var doc = WordprocessingDocument.Open(templateStream, false);

                if(doc.MainDocumentPart?.Document?.Body == null)
                {
                    result.AddError("TEMPLATE_INVALID", "Template is empty");
                    return result;
                }

                var body = doc.MainDocumentPart.Document.Body;

                var allSdts = body.Descendants<SdtElement>().ToList();
                result.TotalContentControlsFound = allSdts.Count;

                result.AddInfo($"Found {allSdts.Count} Content Controls in template");

                // Проверка наличия таблицы-шаблона
                ValidateTemplateTable(body, schema, result);

                // Проверка полей для каждого компонента
                ValidateItemFields(allSdts, schema, result);

                // Проверка мета-полей
                if (schema.MetaFieldTags != null && schema.MetaFieldTags.Any())
                {
                    ValidateMetaFields(allSdts, schema, result);
                }

                // Проверка на лишние Content Controls
                ValidateUnexpectedControls(allSdts, schema, result);

                // Подсчёт ожидаемых Content Controls
                result.ExpectedContentControls = CalculateExpectedControls(schema);

                if (result.TotalContentControlsFound < result.ExpectedContentControls)
                {
                    result.AddWarning(
                        "MISSING_CONTROLS",
                        $"Found {result.TotalContentControlsFound} controls, expected at least {result.ExpectedContentControls}");
                }
            }
            catch (Exception ex)
            {
                result.AddError("VALIDATION_FAILED", $"Validation failed: {ex.Message}");
            }

            return result;
        }



        public async Task<ValidationResult> ValidateAsync(TemplateId templateId, CancellationToken ct = default)
        {
            var result = new ValidationResult();

            try
            {
                //Загружаем схему
                var schema =  _schemaStore.GetSchema(templateId.FormCode);

                // Открываем шаблон 
                await using var templateStream = _templateStore.OpenTemplate(templateId);

                //Копируем в MemoryStream
                var memStream = new MemoryStream();
                await templateStream.CopyToAsync(memStream, ct);
                memStream.Position = 0;

                return Validate(memStream, schema);
            }
            catch(Exception ex)
            {
                result.AddError("VALIDATION_FAILED", $"Validation failed: {ex.Message}");
                return result;
            }
        }

        public async Task<Dictionary<string, ValidationResult>> ValidateAllAsync(CancellationToken ct = default)
        {
            var results = new Dictionary<string, ValidationResult>();

            var formCodes = _schemaStore.GetAvailableFormCodes();

            foreach (var formCode in formCodes)
            {
                if (ct.IsCancellationRequested)
                    break;

                try
                {
                    var templateId = new TemplateId(formCode);
                    var result = await ValidateAsync(templateId, ct);
                    results[formCode] = result;
                }
                catch (Exception ex)
                {
                    var errorResult = new ValidationResult();
                    errorResult.AddError("VALIDATION_FAILED", $"Failed to validate form {formCode}: {ex.Message}");
                    results[formCode] = errorResult;
                }
            }

            return results;
        }

        private void ValidateTemplateTable(Body body, FormSchema schema, ValidationResult result)
        {
            var templateTableTag = schema.TemplateTableTag;

            var templateTableSdt = body.Descendants<SdtElement>()
                .FirstOrDefault(sdt => GetSdtTag(sdt) == templateTableTag);

            if (templateTableSdt == null)
            {
                result.AddError(
                    "TEMPLATE_TABLE_MISSING",
                    $"Template table with tag '{templateTableTag}' not found. " +
                    "This is required for multi-page document generation.");
                return;
            }

            // Проверяем, что внутри SDT есть таблица
            var table = templateTableSdt.Descendants<Table>().FirstOrDefault();
            if (table == null)
            {
                result.AddError(
                    "TEMPLATE_TABLE_NO_TABLE",
                    $"Content Control with tag '{templateTableTag}' does not contain a table");
            }
            else
            {
                result.AddInfo($"Template table '{templateTableTag}' found");
            }
        }

        private void ValidateItemFields(List<SdtElement> allSdts, FormSchema schema, ValidationResult result)
        {
            var requiredFields = schema.GetRequiredFields();

            // Для каждого компонента (1..ItemsPerPage)
            for (int i = 1; i <= schema.ItemsPerPage; i++)
            {
                foreach (var fieldKey in requiredFields)
                {
                    var expectedTag = schema.GetTagForField(fieldKey, i);

                    var sdt = allSdts.FirstOrDefault(s => GetSdtTag(s) == expectedTag);

                    if (sdt == null)
                    {
                        result.AddError(
                            "MISSING_FIELD",
                            $"Missing Content Control for item {i}, field '{fieldKey}'",
                            $"Expected tag: '{expectedTag}'");
                    }
                }
            }
        }

        private void ValidateMetaFields(List<SdtElement> allSdts, FormSchema schema, ValidationResult result)
        {
            if (schema.MetaFieldTags == null)
                return;

            foreach (var (key, tag) in schema.MetaFieldTags)
            {
                var sdt = allSdts.FirstOrDefault(s => GetSdtTag(s) == tag);

                if (sdt == null)
                {
                    result.AddWarning(
                        "MISSING_META_FIELD",
                        $"Missing Content Control for meta field '{key}'",
                        $"Expected tag: '{tag}'");
                }
            }
        }

        private void ValidateUnexpectedControls(List<SdtElement> allSdts, FormSchema schema, ValidationResult result)
        {
            var expectedTags = new HashSet<string>();

            // Добавляем тег таблицы-шаблона
            expectedTags.Add(schema.TemplateTableTag);

            // Добавляем теги для всех полей компонентов
            for (int i = 1; i <= schema.ItemsPerPage; i++)
            {
                foreach (var fieldKey in schema.GetRequiredFields())
                {
                    expectedTags.Add(schema.GetTagForField(fieldKey, i));
                }
            }

            // Добавляем мета-поля
            if (schema.MetaFieldTags != null)
            {
                foreach (var tag in schema.MetaFieldTags.Values)
                {
                    expectedTags.Add(tag);
                }
            }

            // Ищем лишние Content Controls
            var unexpectedSdts = allSdts
                .Where(sdt => !string.IsNullOrEmpty(GetSdtTag(sdt)) &&
                             !expectedTags.Contains(GetSdtTag(sdt)!))
                .ToList();

            foreach (var sdt in unexpectedSdts)
            {
                var tag = GetSdtTag(sdt);
                result.AddWarning(
                    "UNEXPECTED_CONTROL",
                    $"Unexpected Content Control found",
                    $"Tag: '{tag}' (not defined in schema)");
            }
        }

        private int CalculateExpectedControls(FormSchema schema)
        {
            int count = 1; // Таблица-шаблон

            // Поля для каждого компонента
            count += schema.ItemsPerPage * schema.ItemFieldTagTemplates.Count;

            // Мета-поля
            if (schema.MetaFieldTags != null)
            {
                count += schema.MetaFieldTags.Count;
            }

            return count;
        }

        private static string? GetSdtTag(SdtElement sdt)
        {
            return sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value;
        }
    }
}
