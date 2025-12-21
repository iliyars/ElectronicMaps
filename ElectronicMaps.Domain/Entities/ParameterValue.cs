using ElectronicMaps.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Entities
{
    public class ParameterValue : DomainObject
    {
        public int ParameterDefinitionId { get; set; }
        public ParameterDefinition ParameterDefinition { get; set; } = null!;

        /// <summary>Если значение относится к семейству (FormScope.Family).</summary>
        public int? ComponentFamilyId { get; set; }
        public ComponentFamily? ComponentFamily { get; set; }

        /// <summary>Если значение относится к конкретному компоненту (FormScope.Component).</summary>
        public int? ComponentId { get; set; }
        public Component? Component { get; set; }

        // Универсальное хранение значения:
        public string? StringValue { get; set; }
        public double? DoubleValue { get; set; }
        public int? IntValue { get; set; }

        /// <summary>Для ValueKind = WithPins — номера выводов.</summary>
        public string? Pins { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }

    }
}
