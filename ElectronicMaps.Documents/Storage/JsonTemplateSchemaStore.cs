using ElectronicMaps.Documents.Configuration;
using ElectronicMaps.Documents.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ElectronicMaps.Documents.Storage
{
    /// <summary>
    /// Хранилище JSON схем на основе файловой системы
    /// </summary>
    /// <remarks>
    /// <para>
    /// Загружает схемы из JSON файлов в папке, указанной в опциях (SchemasDirectory).
    /// Использует in-memory кэширование для повышения производительности.
    /// </para>
    /// <para>
    /// Потокобезопасен (thread-safe) благодаря использованию SemaphoreSlim.
    /// </para>
    /// </remarks>
    public class JsonTemplateSchemaStore : ITemplateSchemaStore
    {
        private readonly ILogger<JsonTemplateSchemaStore> _logger;
        private readonly DocumentGeneratorOptions _options;
        private readonly Dictionary<string, TemplateSchema> _cache;
        private readonly SemaphoreSlim _cacheLock;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonTemplateSchemaStore(ILogger<JsonTemplateSchemaStore> logger, DocumentGeneratorOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _cache = new Dictionary<string, TemplateSchema>(StringComparer.OrdinalIgnoreCase);
            _cacheLock = new SemaphoreSlim(1, 1);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
        }

        public async Task<List<TemplateSchema>> GetAllSchemaAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Получение всех схем из директории {Directory}", _options.SchemasDirectory);

            if (!Directory.Exists(_options.SchemasDirectory))
            {
                _logger.LogWarning("Директория схем не существует: {Directory}", _options.SchemasDirectory);
                return new List<TemplateSchema>();
            }

            var jsonFiles = Directory.GetFiles(_options.SchemasDirectory, "*.json");

            _logger.LogDebug("Найдено JSON файлов: {Count}", jsonFiles.Length);

            var schemas = new List<TemplateSchema>();

            foreach(var file in jsonFiles)
            {
                try
                {
                    var templateCode = Path.GetFileNameWithoutExtension(file);
                    var schema = await GetSchemaAsync(templateCode, cancellationToken);
                    schemas.Add(schema);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка загрузки схемы из файла {File}", file);
                }
            }

            _logger.LogInformation("Загружено схем: {Count} из {Total}", schemas.Count, jsonFiles.Length);

            return schemas;
        }

        public async Task<TemplateSchema> GetSchemaAsync(string templateCode, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Получение схемы для {TemplateCode}", templateCode);

            // Проверяем кэш (если кэширование включено)
            if (_options.EnableSchemasCaching && _cache.TryGetValue(templateCode, out var cachedSchema))
            {
                _logger.LogTrace("Схема {TemplateCode} взята из кэша", templateCode);
                return cachedSchema;
            }

            // Загружаем из файла с блокировкой
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                // Double-check после получения блокировки
                if (_options.EnableSchemasCaching && _cache.TryGetValue(templateCode, out cachedSchema))
                {
                    _logger.LogTrace("Схема {TemplateCode} взята из кэша (double-check)", templateCode);
                    return cachedSchema;
                }

                var schema = await LoadSchemaFromFileAsync(templateCode, cancellationToken);

                // Добавляем в кэш (если кэширование включено)
                if (_options.EnableSchemasCaching)
                {
                    _cache[templateCode] = schema;
                    _logger.LogTrace("Схема {TemplateCode} добавлена в кэш", templateCode);
                }

                _logger.LogInformation(
                    "Схема загружена: {TemplateCode} ({DisplayName}), маппингов: {MappingCount}",
                    schema.TemplateCode,
                    schema.FieldMappings.Count);

                return schema;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public Task<bool> SchemaExistsAsync(string templateCode)
        {
            // Проверяем кэш
            if (_cache.ContainsKey(templateCode))
            {
                _logger.LogTrace("Схема {TemplateCode} найдена в кэше", templateCode);
                return Task.FromResult(true);
            }

            // Проверяем файл
            var schemaPath = _options.GetSchemaPath(templateCode);
            var exists = File.Exists(schemaPath);

            _logger.LogTrace(
                "Проверка существования схемы {TemplateCode}: {Exists} (путь: {Path})",
                templateCode,
                exists,
                schemaPath);

            return Task.FromResult(exists);
        }

        private async Task<TemplateSchema> LoadSchemaFromFileAsync(
            string templateCode,
            CancellationToken cancellationToken)
        {
            var schemaPath = _options.GetSchemaPath(templateCode);

            _logger.LogDebug("Загрузка схемы из файла: {SchemaPath}", schemaPath);

            if (!File.Exists(schemaPath))
            {
                _logger.LogError("JSON схема не найдена: {SchemaPath}", schemaPath);
                throw new FileNotFoundException($"JSON схема не найдена: {schemaPath}");
            }

            var json = await File.ReadAllTextAsync(schemaPath, cancellationToken);

            _logger.LogTrace("Прочитано {Size} байт из {File}", json.Length, Path.GetFileName(schemaPath));

            var schema = JsonSerializer.Deserialize<TemplateSchema>(json, _jsonOptions);

            if (schema == null)
            {
                _logger.LogError("Не удалось десериализовать схему: {SchemaPath}", schemaPath);
                throw new InvalidOperationException($"Не удалось десериализовать схему: {schemaPath}");
            }

            return schema;

        }

        /// <summary>
        /// Очистить кэш (для тестирования или перезагрузки)
        /// </summary>
        public void ClearCache()
        {
            _cacheLock.Wait();
            try
            {
                var count = _cache.Count;
                _cache.Clear();
                _logger.LogDebug("Кэш схем очищен (было схем: {Count})", count);
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        /// <summary>
        /// Получить количество схем в кэше
        /// </summary>
        public int GetCacheCount()
        {
            return _cache.Count;
        }
    }
}
