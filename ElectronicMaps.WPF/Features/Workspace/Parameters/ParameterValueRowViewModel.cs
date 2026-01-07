using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Domain.Enums;
using System.Globalization;


namespace ElectronicMaps.WPF.Features.Workspace.Parameters
{
    public partial class ParameterValueRowViewModel : ObservableObject
    {
        public int DefinitionId { get; }
        public string Code { get; }
        public string DisplayName { get; }
        public string? Unit { get; }
        public string? Group { get; }
        public ParameterValueKind Kind { get; }
        public bool HasPins { get; }

        // значения (одно из них используется)
        [ObservableProperty]
        private string? stringValue;
        [ObservableProperty]
        private double? doubleValue;
        [ObservableProperty]
        private int? intValue;
        [ObservableProperty]
        private string? pins;

        public ParameterValueRowViewModel(ParameterDefinitionDto def)
        {
            DefinitionId = def.Id;
            Code = def.Code;
            DisplayName = def.DisplayName;
            Unit = def.Unit;
            Kind = def.DataType;
        }

        public ParameterValueInput ToInput() =>
            new ParameterValueInput
            {
                ParameterCode = Code,
                Value = GetValueAsString(),
                Pins = Pins
            };

        /// <summary>
        /// Получить значение в виде строки (в зависимости от типа)
        /// </summary>
        private string? GetValueAsString()
        {
            return Kind switch
            {
                ParameterValueKind.String or ParameterValueKind.WithPins => StringValue,
                ParameterValueKind.Double => DoubleValue?.ToString(CultureInfo.InvariantCulture),
                ParameterValueKind.Int => IntValue?.ToString(CultureInfo.InvariantCulture),
                _ => StringValue
            };
        }

        /// <summary>
        /// Установить значение из строки (для загрузки из БД)
        /// </summary>
        public void SetValueFromString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                StringValue = null;
                DoubleValue = null;
                IntValue = null;
                return;
            }

            switch (Kind)
            {
                case ParameterValueKind.String:
                case ParameterValueKind.WithPins:
                    StringValue = value;
                    break;

                case ParameterValueKind.Double:
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        DoubleValue = d;
                    else
                        StringValue = value; // Fallback
                    break;

                case ParameterValueKind.Int:
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                        IntValue = i;
                    else
                        StringValue = value; // Fallback
                    break;

                default:
                    StringValue = value;
                    break;
            }
        }

        public bool HasValue =>
            !string.IsNullOrWhiteSpace(StringValue)
            || DoubleValue.HasValue
            || IntValue.HasValue;
    }
}
