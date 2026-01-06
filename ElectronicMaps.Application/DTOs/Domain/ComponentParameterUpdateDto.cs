using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Domain
{
    public class ComponentParameterUpdateDto
    {
        public int ComponentId { get; init; }

        public List<ParameterDto> Parameters { get; init; } = new();




    }
}
