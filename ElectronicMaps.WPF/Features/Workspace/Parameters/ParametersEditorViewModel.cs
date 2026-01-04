using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Application.DTOs.Parameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.Parameters
{
    public partial class ParametersEditorViewModel : ObservableObject
    {
        public ObservableCollection<ParameterValueRowViewModel> Rows { get; } = new();

        public ParametersEditorViewModel(IEnumerable<ParameterDefinitionDto> definitions)
        {
            foreach (var def in definitions.OrderBy(d => d.Order))
            {
                Rows.Add(new ParameterValueRowViewModel(def));
            }
        }
        //TODO: Валидация
        public bool HasAnyValue => Rows.Any(r => r.HasValue);

        public bool IsValid => true;

        public IReadOnlyList<ParameterValueInput> BuildInputs()
        {
            return Rows.Where(r => r.HasValue)
                .Select(r => r.ToInput())
                .ToList();
        }
    }
}
