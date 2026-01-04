using ElectronicMaps.Application.DTOs.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Queries.Parameters
{
    public interface IParameterDefinitionReadRepository
    {
        Task<IReadOnlyList<ParameterDefinitionDto>> GetByFormTypeIdAsync(int formTypeId, CancellationToken ct);

        Task<IReadOnlyList<ParameterDefinitionDto>> GetByFormCodeAsync(string formCode, CancellationToken ct);


    }
}
