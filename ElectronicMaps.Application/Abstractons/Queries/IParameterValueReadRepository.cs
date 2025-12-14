using ElectronicMaps.Application.DTO.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IParameterValueReadRepository
    {
        Task<IReadOnlyList<ParameterValueDto>> GetComponentValuesAsync(int componentId, CancellationToken ct);
        Task<IReadOnlyList<ParameterValueDto>> GetFamilyValuesAsync(int familyId, CancellationToken ct); 
    }
}
