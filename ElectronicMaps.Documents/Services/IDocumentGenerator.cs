using ElectronicMaps.Documents.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Documents.Services
{
    /// <summary>
    /// Сервис генерации Word документов из компонентов
    /// </summary>
    /// <remarks>
    /// <para>
    /// Главный интерфейс библиотеки. Предоставляет высокоуровневый API для генерации
    /// Word документов из данных компонентов.
    /// </para>
    /// <para>
    /// Основная реализация: <see cref="WordDocumentGenerator"/>
    /// </para>
    /// </remarks>
    /// <example>
    /// Типичное использование:
    /// <code>
    /// // Регистрация в DI
    /// services.AddWordDocumentGenerator(options =>
    /// {
    ///     options.TemplatesDirectory = "templates";
    ///     options.SchemasDirectory = "schemas";
    /// });
    /// 
    /// // Использование
    /// public class MyService
    /// {
    ///     private readonly IDocumentGenerator _generator;
    ///     
    ///     public MyService(IDocumentGenerator generator)
    ///     {
    ///         _generator = generator;
    ///     }
    ///     
    ///     public async Task CreateDocumentAsync()
    ///     {
    ///         var request = new DocumentGenerationRequest
    ///         {
    ///             FormCode = "FORM_4",
    ///             Components = components,
    ///             OutputPath = "output/result.docx"
    ///         };
    ///         
    ///         var result = await _generator.GenerateAsync(request);
    ///         
    ///         if (result.Success)
    ///         {
    ///             Console.WriteLine($"✓ Создан: {result.OutputPath}");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IDocumentGenerator
    {
        /// <summary>
        /// Сгенерировать Word документ из компонентов
        /// </summary>
        /// <param name="request">Запрос с данными компонентов и настройками</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат генерации (Success/Failure)</returns>
        /// <remarks>
        /// <para>
        /// Полный процесс генерации:
        /// 1. Валидация запроса
        /// 2. Загрузка JSON схемы
        /// 3. Поиск Word шаблона
        /// 4. Вычисление количества таблиц
        /// 5. Копирование шаблона
        /// 6. Клонирование таблиц
        /// 7. Заполнение ячеек
        /// 8. Сохранение результата
        /// </para>
        /// <para>
        /// Использует Result Pattern - не выбрасывает исключения для бизнес-ошибок,
        /// а возвращает GenerationResult с Success = false и ErrorMessage.
        /// </para>
        /// </remarks>
        Task<GenerationResult> GenerateAsync(
        DocumentGenerationRequest request,
        CancellationToken cancellationToken = default);
    }
}
