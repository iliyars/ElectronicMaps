using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Repositories
{
    public class ComponentQueryRepository : IComponentQueryRepository
    {

        private readonly AppDbContext _db;

        public ComponentQueryRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Component?> GetByNameAsync(string canonicalName, CancellationToken ct) =>
            _db.Components
                .Include(c => c.ComponentFamily)
                .FirstOrDefaultAsync(c => c.Name == canonicalName, ct);

        public Task<FormType?> GetFormTypeByCodeAsync(string code, CancellationToken ct) =>
            _db.FormTypes
                .Include(ft => ft.Parameters)
                .FirstOrDefaultAsync(ft => ft.Code == code, ct);

        public async Task<IReadOnlyList<ParameterValue>> GetParameterValuesAsync(int componentId, CancellationToken ct)
        {
            return await _db.ParameterValues
                .Where(v => v.ComponentId == componentId)
                .Include(v => v.ParameterDefinition)
                .ToListAsync(ct);
        }

        public Task<bool> ExistsAsync(string name, CancellationToken ct = default) =>
            _db.Components.AnyAsync(c => c.Name == name, ct);

        public Task<ComponentFamily> GetFamilyByIdAsync(int id, CancellationToken ct) =>
            _db.ComponentFamilies.FirstOrDefaultAsync(f => f.Id == id, ct);

        public Task<ComponentFamily?> GetFamilyByNameAsync(string name, CancellationToken ct) =>
            _db.ComponentFamilies.FirstOrDefaultAsync(f => f.Name == name, ct);

        public Task<List<ComponentFamily>> GetFamiliesByNamesAsync(IEnumerable<string> familyNames,CancellationToken ct = default)
        {
            var names = familyNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToArray();

            return _db.ComponentFamilies
                .Where(f=>names.Contains(f.Name))
                .ToListAsync(ct);
        }
        
        public Task<Component?> GetByIdAsync(int id, CancellationToken ct)
        {
            return _db.Components.FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<IReadOnlyList<ParameterValue>> GetFamilyParameterValuesAsync(int familyId, CancellationToken ct = default)
        {
            return await _db.ParameterValues
                .Where(v => v.ComponentFamilyId == familyId)
                .Include(v => v.ParameterDefinition)
                .ToListAsync(ct);
        }

        public  Task<List<Component>> GetByNamesAsync(IEnumerable<string> canonicalNames,CancellationToken ct = default)
        {
            return  _db.Components
                .Where(c => canonicalNames.Contains(c.CanonicalName))
                .Include(c => c.ComponentFamily)
                .ToListAsync(ct);
        }

        public Task<ComponentFamily?> GetFamilyByIdWithFormAsync(int familyId, CancellationToken ct)
        {
            return  _db.ComponentFamilies
                .Include(f => f.FamilyFormType)
                .ThenInclude(ft => ft.Parameters)
                .FirstOrDefaultAsync(f => f.Id == familyId, ct);
        }

        public  Task<Component?> GetComponentByIdWithFormAsync(int componentId, CancellationToken ct)
        {
            return _db.Components
                .Include(c => c.FormType)
                .ThenInclude(ft => ft.Parameters)
                .Include(c => c.ComponentFamily)
                .FirstOrDefaultAsync(c => c.Id == componentId, ct);
        }
    }
}
