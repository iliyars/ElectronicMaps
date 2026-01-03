using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Domain.Enums;


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
            new ParameterValueInput(
                ParameterDefinitionId: DefinitionId,
                StringValue: StringValue,
                DoubleValue: DoubleValue,
                IntValue: IntValue,
                Pins: Pins);

        public bool HasValue =>
            !string.IsNullOrWhiteSpace(StringValue)
        || DoubleValue.HasValue
        || IntValue.HasValue;
    }
}
