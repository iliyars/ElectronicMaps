using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Repositories
{
    public interface IComponentQueryRepository
    {
        Task<Component?> GetByIdAsync(int id, CancellationToken ct);

        Task<Component?> GetByNameAsync(string name, CancellationToken ct);
        Task<ComponentFamily> GetFamilyByIdAsync(int id, CancellationToken ct);
        Task<ComponentFamily?> GetFamilyByNameAsync(string name, CancellationToken ct);

        Task<FormType?> GetFormTypeByCodeAsync(string code, CancellationToken ct);
        Task<IReadOnlyList<ParameterValue>> GetParameterValuesAsync(int componentId, CancellationToken ct);
        Task<IReadOnlyList<ParameterValue>> GetFamilyParameterValuesAsync(int familyId, CancellationToken ct = default);

        public Task<List<Component>> GetByNamesAsync(IEnumerable<string> canonicalNames,CancellationToken ct = default);
        Task<bool> ExistsAsync(string canonicalName, CancellationToken ct = default);





    }
}
