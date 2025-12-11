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
       
        public string Code { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string? TemplateKey { get; set; }

        public ICollection<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();


    }
}
