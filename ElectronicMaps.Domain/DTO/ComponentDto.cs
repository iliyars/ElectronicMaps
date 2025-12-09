using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.DTO
{
    public class ComponentDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
        public string FormCode { get; init; } = default!;
        public string FamilyName { get; init; } = default!;
        public IReadOnlyList<ParameterDto> Parameters { get; init; } = default!;
    }
}
