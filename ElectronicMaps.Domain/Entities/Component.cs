using ElectronicMaps.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Entities
{
    public class Component : DomainObject
    {
        public int ComponentFamilyId { get; set; }
        public ComponentFamily ComponentFamily { get; set; } = null!;

        public string Name { get; set; }

        /// <summary>Нормализованное имя для поиска (опционально).</summary>
        public string? CanonicalName { get; set; }

        public int? FormTypeId { get;set; }
        public FormType? FormType { get; set; }
        public string FormCode { get; set; }

        public ICollection<ParameterValue> ParameterValues { get; set; } = new List<ParameterValue>();


    }
}
