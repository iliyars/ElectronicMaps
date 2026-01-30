using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Documents.Models
{
    public class GenerationResult
    {
        // <summary>
        /// Успешно ли выполнена генерация?
        /// </summary>
        public required bool Success { get; init; }

        /// <summary>
        /// Путь к сгенерированному файлу (если успешно)
        /// </summary>
        public string? OutputPath { get; init; }

        /// <summary>
        /// Количество обработанных компонентов
        /// </summary>
        public int ProcessedComponents { get; init; }

        /// <summary>
        /// Количество созданных таблиц
        /// </summary>
        public int CreatedTables { get; init; }

        /// <summary>
        /// Количество записанных ячеек
        /// </summary>
        public int WrittenCells { get; init; }

        /// <summary>
        /// Сообщение об ошибке (если Success = false)
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Список предупреждений (необязательно)
        /// </summary>
        public List<string> Warnings { get; init; } = new();

        /// <summary>
        /// Создать успешный результат
        /// </summary>
        /// <param name="outputPath">Путь к созданному файлу</param>
        /// <param name="processedComponents">Количество обработанных компонентов</param>
        /// <param name="createdTables">Количество созданных таблиц</param>
        /// <param name="writtenCells">Количество записанных ячеек</param>
        /// <returns>Успешный результат</returns>
        public static GenerationResult Ok(
            string outputPath,
            int processedComponents,
            int createdTables,
            int writtenCells)
        {
            return new GenerationResult
            {
                Success = true,
                OutputPath = outputPath,
                ProcessedComponents = processedComponents,
                CreatedTables = createdTables,
                WrittenCells = writtenCells
            };
        }

        /// <summary>
        /// Создать результат с ошибкой
        /// </summary>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <returns>Результат с ошибкой</returns>
        public static GenerationResult Failure(string errorMessage)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ProcessedComponents = 0,
                CreatedTables = 0,
                WrittenCells = 0
            };
        }

        /// <summary>
        /// Добавить предупреждение
        /// </summary>
        /// <param name="warning">Текст предупреждения</param>
        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        public override string ToString()
        {
            if (Success)
            {
                return $"Success: {ProcessedComponents} компонентов → {CreatedTables} таблиц → {WrittenCells} ячеек";
            }
            else
            {
                return $"Failure: {ErrorMessage}";
            }
        }
    }
}
