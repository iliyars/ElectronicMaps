using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Rendering.Validation
{

    public record ValidationError(string Code, string Message, string? Location = null);
    public record ValidationWarning(string Code, string Message, string? Location = null);
    public record ValidationInfo(string Message);


    public class ValidationResult
    {
        /// <summary>
        /// Признак успешной валидации
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Список ошибок валидации
        /// </summary>
        public List<ValidationError> Errors { get; } = new();

        /// <summary>
        /// Список предупреждений
        /// </summary>
        public List<ValidationWarning> Warnings { get; } = new();

        /// <summary>
        /// Информационные сообщения
        /// </summary>
        public List<ValidationInfo> Infos { get; } = new();

        /// <summary>
        /// Общее количество найденных Content Controls в шаблоне
        /// </summary>
        public int TotalContentControlsFound { get; set; }

        // <summary>
        /// Количество ожидаемых Content Controls по схеме
        /// </summary>
        public int ExpectedContentControls { get; set; }

        /// <summary>
        /// Добавляет ошибку
        /// </summary>
        public void AddError(string code, string message, string? location = null)
        {
            Errors.Add(new ValidationError(code, message, location));
        }

        /// <summary>
        /// Добавляет предупреждение
        /// </summary>
        public void AddWarning(string code, string message, string? location = null)
        {
            Warnings.Add(new ValidationWarning(code, message, location));
        }

        /// <summary>
        /// Добавляет информационное сообщение
        /// </summary>
        public void AddInfo(string message)
        {
            Infos.Add(new ValidationInfo(message));
        }

        ///
        public string GetReport()
        {
            var report = new System.Text.StringBuilder();

            report.AppendLine("=== Template Validation Report ===");
            report.AppendLine();

            report.AppendLine($"Status: {(IsValid ? "✓ VALID" : "✗ INVALID")}");
            report.AppendLine($"Content Controls: {TotalContentControlsFound} found, {ExpectedContentControls} expected");
            report.AppendLine();

            if (Errors.Any())
            {
                report.AppendLine($"❌ Errors ({Errors.Count}):");
                foreach (var error in Errors)
                {
                    report.AppendLine($"  [{error.Code}] {error.Message}");
                    if (!string.IsNullOrEmpty(error.Location))
                        report.AppendLine($"    Location: {error.Location}");
                }
                report.AppendLine();
            }

            if (Warnings.Any())
            {
                report.AppendLine($"⚠️  Warnings ({Warnings.Count}):");
                foreach (var warning in Warnings)
                {
                    report.AppendLine($"  [{warning.Code}] {warning.Message}");
                    if (!string.IsNullOrEmpty(warning.Location))
                        report.AppendLine($"    Location: {warning.Location}");
                }
                report.AppendLine();
            }

            if (Infos.Any())
            {
                report.AppendLine($"ℹ️  Info ({Infos.Count}):");
                foreach (var info in Infos)
                {
                    report.AppendLine($"  {info.Message}");
                }
                report.AppendLine();
            }

            return report.ToString();
        }
    }
}
