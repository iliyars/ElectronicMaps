using ElectronicMaps.Domain.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Entities
{
    public class Component : DomainObject
    {
        public string Name { get; set; } = null!;
        /// <summary>Нормализованное имя для поиска (опционально).</summary>
        public string? CanonicalName { get; set; }

        public int ComponentFamilyId { get; set; }
        public ComponentFamily ComponentFamily { get; set; } = null!;

        public int? FormTypeId { get;set; }
        public FormType? FormType { get; set; }

        public ICollection<ParameterValue> ParameterValues { get; set; } = new List<ParameterValue>();


    }
}
