using ElectronicMaps.Domain.Enums;
using ElectronicMaps.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Entities
{
    public class ComponentFamily : DomainObject
    {
        public string Name { get; set; } = null!;
        public int? FamilyFormTypeId { get; set; } 
        public FormType? FamilyFormType { get; set; }
        public ICollection<Component> Components { get; set; } = new List<Component>();
        public ICollection<ParameterValue> ParameterValues { get;set; } = new List<ParameterValue>();
    }
}
