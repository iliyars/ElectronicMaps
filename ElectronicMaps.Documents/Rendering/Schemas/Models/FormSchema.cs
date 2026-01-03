using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Rendering.Schemas.Models
{
    /// <summary>
    /// Описание формы, сколько элементов на странице и как мапить поля -> SDT Tag.
    /// </summary>
    public class FormSchema
    {
        /// <summary>
        /// Код формы (например, "4", "5", "6")
        /// </summary>
        [JsonPropertyName("formCode")]
        public required string FormCode { get; set; }

        /// <summary>
        /// Количество компонентов на одной странице
        /// </summary>
        [JsonPropertyName("itemsPerPage")]
        public required int ItemsPerPage { get; set; }

        /// <summary>
        /// Маппинг "поле источника" → "шаблон SDT Tag".
        /// Шаблон должен содержать {i} для подстановки номера компонента.
        /// Пример TagTemplate: "F04.Item[{i}].Name"
        /// </summary>
        [JsonPropertyName("itemFieldTemplates")]
        public required Dictionary<string, string> ItemFieldTagTemplates { get; set; }

        /// <summary>
        /// Общие (не item) поля: "ключ" → "SDT Tag".
        /// Пример: "Meta.DocumentNumber" → "F04.Meta.DocumentNumber"
        /// </summary>
        [JsonPropertyName("metaFieldTags")]
        public Dictionary<string, string>? MetaFieldTags { get; init; }

        /// <summary>
        /// SDT Tag для таблицы-шаблона, которая будет клонироваться для множественных страниц.
        /// По умолчанию: "TemplateTable"
        /// </summary>
        [JsonPropertyName("templateTableTag")]
        public string TemplateTableTag { get; init; } = "TemplateTable";

        /// <summary>
        /// Описание формы (для документации, не используется в коде)
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; init; }

        /// <summary>
        /// Версия схемы (для обратной совместимости)
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version { get; init; }

        /// <summary>
        /// Валидирует схему на корректность
        /// </summary>
        /// <exception cref="InvalidOperationException">Если схема некорректна</exception>
        public void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FormCode))
                errors.Add("FormCode cannot be empty");

            if (ItemsPerPage <= 0)
                errors.Add($"ItemsPerPage must be greater than 0, got: {ItemsPerPage}");

            if (ItemFieldTagTemplates == null || ItemFieldTagTemplates.Count == 0)
                errors.Add("ItemFieldTagTemplates cannot be empty");
            else
            {
                // Проверка, что все шаблоны содержат {i}
                var invalidTemplates = ItemFieldTagTemplates
                    .Where(kvp => string.IsNullOrWhiteSpace(kvp.Value) ||
                                  !kvp.Value.Contains("{i}", StringComparison.Ordinal))
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (invalidTemplates.Any())
                    errors.Add($"Tag templates must contain '{{i}}' placeholder: {string.Join(", ", invalidTemplates)}");
            }

            if (string.IsNullOrWhiteSpace(TemplateTableTag))
                errors.Add("TemplateTableTag cannot be empty");

            if (errors.Any())
                throw new InvalidOperationException(
                    $"Schema validation failed for form '{FormCode}':\n" +
                    string.Join("\n", errors.Select(e => $"- {e}")));
        }

        /// <summary>
        /// Получает SDT Tag для конкретного поля и номера компонента
        /// </summary>
        /// <param name="fieldKey">Ключ поля (например, "Name", "Param.TempMin")</param>
        /// <param name="itemIndex">Номер компонента (1-based: 1, 2, 3...)</param>
        /// <returns>Готовый SDT Tag (например, "F04.Item[1].Name")</returns>
        public string GetTagForField(string fieldKey, int itemIndex)
        {
            if (!ItemFieldTagTemplates.TryGetValue(fieldKey, out var template))
                throw new KeyNotFoundException($"Field '{fieldKey}' not found in schema for form '{FormCode}'");

            return template.Replace("{i}", itemIndex.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// Проверяет, есть ли маппинг для указанного поля
        /// </summary>
        public bool HasFieldMapping(string fieldKey)
        {
            return ItemFieldTagTemplates.ContainsKey(fieldKey);
        }

        /// <summary>
        /// Получает все ключи полей, которые нужно заполнить
        /// </summary>
        public IReadOnlyCollection<string> GetRequiredFields()
        {
            return ItemFieldTagTemplates.Keys.ToList().AsReadOnly();
        }
    }
}
