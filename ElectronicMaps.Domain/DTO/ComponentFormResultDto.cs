using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.DTO
{
    public class ComponentFormResultDto
    {

        public int? ComponentId { get; set; }

        /// <summary>
        /// Name of component as provided by source
        /// </summary>
        public string ComponentName { get; set; } = default!;

        public string? FormCode { get; set; }

        /// <summary>
        /// Indicates whether the component was found in the database.
        /// </summary>
        public bool Found { get; set; }

        /// <summary>
        /// Parameters resolved for this component.
        /// Empty if the component was not found or the form could not be built.
        /// </summary>
        public IReadOnlyList<ParameterDto> Parameters { get; set; } = Array.Empty<ParameterDto>();



    }
}
