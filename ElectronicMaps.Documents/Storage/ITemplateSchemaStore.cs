using ElectronicMaps.Documents.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Documents.Storage
{
    /// <summary>
    /// Хранилище JSON схем шаблонов
    /// </summary>
    /// <remarks>
    /// <para>
    /// Интерфейс для работы с хранилищем схем шаблонов.
    /// Абстрагирует источник схем (файловая система, БД, API, и т.д.)
    /// </para>
    /// <para>
    /// Основная реализация: <see cref="JsonTemplateSchemaStore"/> - загрузка из JSON файлов.
    /// </para>
    /// </remarks>
    public interface ITemplateSchemaStore
    {
        /// <summary>
        /// Получить схему по коду шаблона
        /// </summary>
        /// <param name="templateCode">Код шаблона (например, "FORM_4")</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Схема шаблона</returns>
        /// <exception cref="FileNotFoundException">Схема не найдена</exception>
        /// <exception cref="InvalidOperationException">Ошибка десериализации или валидации</exception>
        /// <example>
        /// <code>
        /// var schema = await schemaStore.GetSchemaAsync("FORM_4");
        /// Console.WriteLine($"Схема: {schema.DisplayName}");
        /// Console.WriteLine($"Компонентов на таблицу: {schema.ComponentsPerTable}");
        /// </code>
        /// </example>
        Task<TemplateSchema> GetSchemaAsync(string templateCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все доступные схемы
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список всех схем</returns>
        /// <remarks>
        /// Полезно для:
        /// - Отображения списка доступных форм в UI
        /// - Предзагрузки всех схем при старте приложения
        /// - Валидации наличия всех необходимых схем
        /// </remarks>
        /// <example>
        /// <code>
        /// var schemas = await schemaStore.GetAllSchemasAsync();
        /// foreach (var schema in schemas)
        /// {
        ///     Console.WriteLine($"{schema.TemplateCode}: {schema.DisplayName}");
        /// }
        /// </code>
        /// </example>
        Task<List<TemplateSchema>> GetAllSchemaAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование схемы
        /// </summary>
        /// <param name="templateCode">Код шаблона</param>
        /// <returns>true если схема существует, иначе false</returns>
        /// <remarks>
        /// Быстрая проверка существования схемы без её загрузки.
        /// Полезно для валидации перед запросом генерации.
        /// </remarks>
        /// <example>
        /// <code>
        /// if (await schemaStore.SchemaExistsAsync("FORM_4"))
        /// {
        ///     var schema = await schemaStore.GetSchemaAsync("FORM_4");
        ///     // Работаем со схемой
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Схема FORM_4 не найдена");
        /// }
        /// </code>
        /// </example>
        Task<bool> SchemaExistsAsync(string templateCode);
    }
}
