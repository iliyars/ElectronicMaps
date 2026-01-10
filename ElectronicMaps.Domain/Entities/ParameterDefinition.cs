using ElectronicMaps.Domain.Common;
using ElectronicMaps.Domain.Enums;

namespace ElectronicMaps.Domain.Entities
{

    /// <summary>
    /// Тип данных для валидации
    /// </summary>
    public enum ParameterDataType
    {
        String = 0,      // Любой текст (default)
        Integer = 1,     // Целое число
        Decimal = 2,     // Дробное число
        Range = 3,       // Диапазон "100-200"
        Boolean = 4,     // да/нет
        Enumeration = 5  // Один из списка
    }

    public class ParameterDefinition : DomainObject
    {

        public int FormTypeId { get; set; }
        public FormType FormType { get; set; } = null!;

        /// <summary>Код параметра внутри формы, например "DcVoltage", "SupplayVoltage".</summary>
        public required string Code { get; set; } = "";

        /// <summary>Человекочитаемое имя параметра.</summary>
        public required string DisplayName { get; set; } = "";

        /// <summary>Единицы измерения ("В", "нс", "мА" и т.д.).</summary>
        public string? Unit { get; set; }

        /// <summary>Тип значения.</summary>
        public ParameterValueKind ValueKind { get; set; } = ParameterValueKind.String;

        /// <summary>Порядок отображения в форме.</summary>
        public int Order { get; set; }

        public ICollection<ParameterValue> ParameterValues { get; set; } = new List<ParameterValue>();

        #region Validation

        public ParameterDataType? DataType { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ValidationPattern { get; set; }
        public string? ValidationMessage { get; set; }
        public bool IsRequired { get; set; } = false;

        #endregion

    }
}
