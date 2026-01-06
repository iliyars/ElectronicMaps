using ElectronicMaps.Domain.Common;
using ElectronicMaps.Domain.Enums;

namespace ElectronicMaps.Domain.Entities
{
    public class ParameterDefinition : DomainObject
    {

        public int FormTypeId { get; set; }
        public FormType FormType { get; set; } = null!;

        /// <summary>Код параметра внутри формы, например "DcVoltage", "SupplayVoltage".</summary>
        public string Code { get; set; } = "";

        /// <summary>Человекочитаемое имя параметра.</summary>
        public string DisplayName { get; set; } = "";

        /// <summary>Единицы измерения ("В", "нс", "мА" и т.д.).</summary>
        public string? Unit { get; set; }

        /// <summary>Тип значения.</summary>
        public ParameterValueKind ValueKind { get; set; }

        /// <summary>Порядок отображения в форме.</summary>
        public int Order { get; set; }

        /// <summary>Группа/секция (опционально, для UI).</summary>
        public string? Group { get; set; }


    }
}
