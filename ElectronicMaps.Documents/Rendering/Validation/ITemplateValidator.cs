using ElectronicMaps.Documents.Core.Models;
using ElectronicMaps.Documents.Rendering.Schemas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Rendering.Validation
{
    public interface ITemplateValidator
    {
        /// <summary>
        /// Валидирует шаблон Word на соответствие схеме
        /// </summary>
        /// <param name="templateStream">Поток с шаблоном Word</param>
        /// <param name="schema">Схема формы</param>
        /// <returns>Результат валидации</returns>
        ValidationResult Validate(Stream templateStream, FormSchema schema);

        /// <summary>
        /// Валидирует шаблон по идентификатору
        /// </summary>
        /// <param name="templateId">Идентификатор шаблона</param>
        /// <returns>Результат валидации</returns>
        Task<ValidationResult> ValidateAsync(TemplateId templateId, CancellationToken ct = default);

        /// <summary>
        /// Валидирует все доступные шаблоны
        /// </summary>
        /// <returns>Словарь: код формы → результат валидации</returns>
        Task<Dictionary<string, ValidationResult>> ValidateAllAsync(CancellationToken ct = default);
    }
}
