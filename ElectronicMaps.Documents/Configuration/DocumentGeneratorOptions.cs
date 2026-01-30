using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Documents.Configuration
{
    /// <summary>
    /// Настройки генератора Word документов (Options Pattern)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Используется для конфигурации путей к шаблонам, схемам и выходным файлам.
    /// Регистрируется через IOptions&lt;DocumentGeneratorOptions&gt; в DI контейнере.
    /// </para>
    /// <example>
    /// Регистрация в DI:
    /// <code>
    /// services.Configure&lt;DocumentGeneratorOptions&gt;(options =>
    /// {
    ///     options.TemplatesDirectory = "templates";
    ///     options.SchemasDirectory = "schemas";
    ///     options.OutputDirectory = "output";
    /// });
    /// </code>
    /// 
    /// Или через appsettings.json:
    /// <code>
    /// {
    ///   "DocumentGenerator": {
    ///     "TemplatesDirectory": "C:/Data/Templates",
    ///     "SchemasDirectory": "C:/Data/Schemas"
    ///   }
    /// }
    /// 
    /// services.Configure&lt;DocumentGeneratorOptions&gt;(
    ///     Configuration.GetSection("DocumentGenerator"));
    /// </code>
    /// </example>
    /// </remarks>
    public class DocumentGeneratorOptions
    {
        /// <summary>
        /// Путь к папке с Word шаблонами (.docx файлы)
        /// </summary>
        /// <remarks>
        /// Может быть относительным или абсолютным путём.
        /// В этой папке должны находиться файлы шаблонов, например: FORM_4.docx, FORM_5.docx
        /// </remarks>
        public string TemplatesDirectory { get; set; } = "Templates";

        /// <summary>
        /// Путь к папке с JSON схемами
        /// </summary>
        /// <remarks>
        /// Может быть относительным или абсолютным путём.
        /// В этой папке должны находиться JSON файлы схем, например: FORM_4.json, FORM_5.json
        /// </remarks>
        public string SchemasDirectory { get; set; } = "Schemas";

        /// <summary>
        /// Путь к папке для выходных файлов (по умолчанию "output")
        /// </summary>
        /// <remarks>
        /// Используется как базовый путь если в запросе указан относительный путь.
        /// Если CreateOutputDirectoryIfNotExists = true, папка будет создана автоматически.
        /// </remarks>
        public string OutputDirectory { get; set; } = "Output";

        /// <summary>
        /// Автоматически создавать выходную папку если не существует?
        /// </summary>
        /// <remarks>
        /// true (по умолчанию) = создать папку автоматически
        /// false = выбросить исключение если папка не существует
        /// </remarks>
        public bool CreateOutputDirectoryIfNotExists { get; set; } = true;

        /// <summary>
        /// Добавлять разрывы страниц между клонированными таблицами?
        /// </summary>
        /// <remarks>
        /// true = каждая таблица на новой странице
        /// false (по умолчанию) = таблицы идут подряд
        /// </remarks>
        public bool InsertPageBreaks { get; set; } = true;

        /// <summary>
        /// Включить кэширование загруженных схем?
        /// </summary>
        /// <remarks>
        /// true (по умолчанию) = схемы загружаются один раз и кэшируются в памяти
        /// false = схемы загружаются каждый раз из файла
        /// </remarks>
        public bool EnableSchemasCaching { get; set; } = true;

        /// <summary>
        /// Валидация настроек
        /// </summary>
        /// <exception cref="InvalidOperationException">Если настройки некорректны</exception>
        public void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(TemplatesDirectory))
            {
                errors.Add("TemplatesDirectory не может быть пустым");
            }

            if (string.IsNullOrWhiteSpace(SchemasDirectory))
            {
                errors.Add("SchemasDirectory не может быть пустым");
            }

            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                errors.Add("OutputDirectory не может быть пустым");
            }

            if (errors.Any())
            {
                throw new InvalidOperationException(
                    $"Невалидные настройки DocumentGeneratorOptions: {string.Join(", ", errors)}");
            }
        }

        /// <summary>
        /// Получить абсолютный путь к шаблону
        /// </summary>
        /// <param name="fileName">Имя файла шаблона (например, "FORM_4.docx")</param>
        /// <returns>Полный путь к файлу</returns>
        public string GetTemplatePath(string fileName)
        {
            return Path.Combine(TemplatesDirectory, fileName);
        }

        /// <summary>
        /// Получить абсолютный путь к схеме
        /// </summary>
        /// <param name="templateCode">Код шаблона (например, "FORM_4")</param>
        /// <returns>Полный путь к JSON файлу</returns>
        public string GetSchemaPath(string templateCode)
        {
            return Path.Combine(SchemasDirectory, $"{templateCode}.json");
        }

        /// <summary>
        /// Получить абсолютный путь к выходному файлу
        /// </summary>
        /// <param name="fileName">Имя файла (например, "result.docx")</param>
        /// <returns>Полный путь к файлу</returns>
        public string GetOutputPath(string fileName)
        {
            return Path.Combine(OutputDirectory, fileName);
        }

        public override string ToString()
        {
            return $"Templates: {TemplatesDirectory}, Schemas: {SchemasDirectory}, Output: {OutputDirectory}";
        }
    }
}
