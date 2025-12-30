using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.OpenXmlWord.Forms.Schemas
{
    /// <summary>
    /// Описание формы, сколько элементов на странице и как мапить поля -> SDT Tag.
    /// </summary>
    public class FormSchema
    {
        public required string FormCode { get; set; }
        public required int ItemsPerPage { get; set; }

        /// <summary>
        /// Маппинг “поле источника” -> “шаблон SDT Tag”.
        /// Пример TagTemplate: "F04.Item[{i}].Name"
        /// </summary>
        public required Dictionary<string, string> ItemFieldTagTemplates { get; set; }

        /// <summary>
        /// Общие (не item) поля: “ключ” -> “SDT Tag”.
        /// </summary>
        public Dictionary<string, string>? MetaFieldTags { get; init; }
    }
}
