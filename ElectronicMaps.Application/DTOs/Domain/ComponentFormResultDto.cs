using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Domain
{
    public record ComponentFormResultDto
    {
        /// <summary>
        /// Имя компонента
        /// </summary>
        public required string ComponentName { get; init; }

        /// <summary>
        /// ID компонента в БД (если найден)
        /// </summary>
        public int? ComponentId { get; init; }

        /// <summary>
        /// Код формы компонента (FORM_4, FORM_5 и т.д.)
        /// </summary>
        public string? ComponentFormCode { get; init; }

        /// <summary>
        /// Название формы компонента
        /// </summary>
        public string? ComponentFormName { get; init; }

        /// <summary>
        /// ID семейства (если найдено)
        /// </summary>
        public int? FamilyId { get; init; }

        /// <summary>
        /// Имя семейства
        /// </summary>
        public string? FamilyName { get; init; }

        /// <summary>
        /// Код формы семейства
        /// </summary>
        public string? FamilyFormCode { get; init; }

        /// <summary>
        /// Название формы семейства
        /// </summary>
        public string? FamilyFormName { get; init; }

        /// <summary>
        /// Есть ли определённая форма (компонента или семейства)
        /// </summary>
        public bool HasDefinedForm =>
            !string.IsNullOrWhiteSpace(ComponentFormCode) ||
            !string.IsNullOrWhiteSpace(FamilyFormCode);

        /// <summary>
        /// Приоритетный код формы (сначала компонента, потом семейства)
        /// </summary>
        public string? PrimaryFormCode => ComponentFormCode ?? FamilyFormCode;

        /// <summary>
        /// Приоритетное название формы
        /// </summary>
        public string? PrimaryFormName => ComponentFormName ?? FamilyFormName;
    }
}
