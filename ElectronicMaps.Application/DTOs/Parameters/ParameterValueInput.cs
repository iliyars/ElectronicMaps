using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Parameters
{
    public class ParameterValueInput
    {
        public required string ParameterCode { get; set; }

        // <summary>
        /// Значение параметра (в виде строки)
        /// Будет преобразовано в нужный тип (String/Double/Int) в зависимости от ParameterDefinition.ValueKind
        /// Например: "10k", "3.3", "100"
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Номера выводов (только для ParameterValueKind.WithPins)
        /// Например: "1,2,3" или "VCC,GND"
        /// </summary>
        public string? Pins { get; set; }
    }
}
