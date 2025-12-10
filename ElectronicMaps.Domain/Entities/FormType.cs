using ElectronicMaps.Domain.Enums;
using ElectronicMaps.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Entities
{
    public class FormType : DomainObject
    {
        /// <summary>Уникальный код формы: "FORM_RESISTOR", "FORM_CHIP", "FORM_64".</summary>
        public string Code { get; set; } = "";

        /// <summary>Отображаемое имя: "Форма резистора", "Форма микросхемы" и т.п.</summary>
        public string DisplayName { get; set; } = "";

        /// <summary>Ключ/путь к Word-шаблону (если будет использоваться).</summary>
        public string? TemplateKey { get; set; }

        public ICollection<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();


    }
}
