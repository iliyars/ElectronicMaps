using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Application.DTO.Parameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Features.Workspace.ViewModels.Parameters
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

        public IReadOnlyCollection<ParameterValueInput> BuildInput()
        {
            return Rows.Where(r => r.HasValue)
                .Select(r => r.ToInput())
                .ToList();
        }




    }
}
