using ElectronicMaps.Application.DTO.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IParameterDefinitionReadRepository
    {
        Task<IReadOnlyList<ParameterDefinitionDto>> GetByFormTypeIdAsync(int formTypeId, CancellationToken ct);

        Task<IReadOnlyList<ParameterDefinitionDto>> GetByFormCodeAsync(string formCode, CancellationToken ct);


    }
}
