using DocumentFormat.OpenXml.Wordprocessing;
using ElectronicMaps.Documents.Rendering.Schemas.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Rendering.Schemas.Store
{
    public class JsonSchemaStore : ISchemaStore
    {
        private readonly string _schemasRoot;
        private readonly ConcurrentDictionary<string, FormSchema> _cache = new();
        private readonly JsonSerializerOptions _jsonOptionos;

        public JsonSchemaStore(string schemasRoot) 
        {
            _schemasRoot = schemasRoot ?? throw new ArgumentNullException(nameof(schemasRoot));

            _jsonOptionos = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
        }

        public FormSchema GetSchema(string formCode)
        {
            if (string.IsNullOrWhiteSpace(formCode))
                throw new ArgumentException("Form code cannot be empty", nameof(formCode));

            return _cache.GetOrAdd(formCode, LoadSchema);
        }

        public bool HasSchema(string formCode)
        {
            if(string.IsNullOrWhiteSpace(formCode))
                return false;

            var filePath = GetSchemaFilePath(formCode);
            return File.Exists(filePath);
        }

        public IReadOnlyCollection<string> GetAvailableFormCodes()
        {
            if (!Directory.Exists(_schemasRoot))
                return Array.Empty<string>();

            var schemaFiles = Directory.GetFiles(_schemasRoot, "Form*.schema.json");

            var formCodes = schemaFiles
                .Select(Path.GetFileNameWithoutExtension) // "Form04.schema"
                .Select(name => name?.Replace("Form", "").Replace(".schema", "")) // "04"
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Cast<string>()
                .ToList();

            return formCodes.AsReadOnly();
        }

        /// <summary>
        /// Очищает кеш схем (полезно для тестов или hot-reload)
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Принудительно перезагружает схему из файла
        /// </summary>
        public FormSchema ReloadSchema(string formCode)
        {
            if (string.IsNullOrWhiteSpace(formCode))
                throw new ArgumentException("Form code cannot be empty", nameof(formCode));

            _cache.TryRemove(formCode, out _);
            return LoadSchema(formCode);
        }

        private FormSchema LoadSchema(string formCode)
        {
            var filePath = GetSchemaFilePath(formCode);

            if (!File.Exists(filePath))
                throw new SchemaNotFoundException(formCode, $"Schema file not found: {filePath}");

            try
            {
                var json = File.ReadAllText(filePath);
                var schema = JsonSerializer.Deserialize<FormSchema>(json, _jsonOptionos);

                if (schema == null)
                    throw new SchemaNotFoundException(
                        formCode,
                        $"Failed to deserialize schema from file: {filePath}");

                ValidateSchema(schema, formCode, filePath);

                return schema;
            }
            catch (JsonException ex)
            {
                throw new SchemaNotFoundException(
                    formCode,
                    $"Invalid JSON in schema file: {filePath}",
                    ex);
            }
            catch(Exception ex)
            {
                throw new SchemaNotFoundException(
                    formCode,
                    $"Error loading schema from file: {filePath}",
                    ex);
            }
        }

        private void ValidateSchema(FormSchema schema, string expectedFormCode, string filePath)
        {
            // Проверка соответствия FormCode
            if (!string.Equals(schema.FormCode, expectedFormCode, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Schema FormCode mismatch in file '{filePath}'. " +
                    $"Expected: '{expectedFormCode}', Found: '{schema.FormCode}'");

            // Проверка ItemsPerPage
            if (schema.ItemsPerPage <= 0)
                throw new InvalidOperationException(
                    $"ItemsPerPage must be greater than 0 in schema '{expectedFormCode}'. " +
                    $"File: {filePath}");

            // Проверка наличия шаблонов полей
            if (schema.ItemFieldTagTemplates == null || schema.ItemFieldTagTemplates.Count == 0)
                throw new InvalidOperationException(
                    $"ItemFieldTagTemplates cannot be empty in schema '{expectedFormCode}'. " +
                    $"File: {filePath}");

            var invalidTemplates = schema.ItemFieldTagTemplates
                .Where(kvp => !kvp.Value.Contains("{i}", StringComparison.Ordinal))
                .Select(kvp => kvp.Key)
                .ToList();

            if (invalidTemplates.Any())
                throw new InvalidOperationException(
                    $"The following tag templates must contain '{{i}}' placeholder: " +
                    $"{string.Join(", ", invalidTemplates)}. File: {filePath}");
        }

        private string GetSchemaFilePath(string formCode)
        {
            return Path.Combine(_schemasRoot, $"Form{formCode}.schema.json");
        }

        private void ValidateSchemasDirectory()
        {
            if (!Directory.Exists(_schemasRoot))
            {
                throw new DirectoryNotFoundException(
                    $"Schemas directory not found: {_schemasRoot}");
            }
        }
    }
}
