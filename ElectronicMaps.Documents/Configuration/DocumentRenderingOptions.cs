namespace ElectronicMaps.Documents.Configuration
{
    /// <summary>
    /// Опции для конфигурации системы рендеринга документов
    /// </summary>
    public class DocumentRenderingOptions
    {
        /// <summary>
        /// Путь к корневой директории со схемами форм (JSON Schema файлы)
        /// </summary>
        public required string SchemasPath { get; set; }

        /// <summary>
        /// Путь к корневой директории с шаблонами документов Word
        /// </summary>
        public required string TemplatesPath { get; set; }

        /// <summary>
        /// Маркер таблицы-шаблона для клонирования при многостраничных документах
        /// По умолчанию: "TemplateTable"
        /// </summary>
        public string TemplateTableMarker { get; set; } = "TemplateTable";

        /// <summary>
        /// Включить валидацию шаблонов при загрузке
        /// </summary>
        public bool ValidateTemplates { get; set; } = true;
    }
}
