using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Components;
using Microsoft.EntityFrameworkCore;

namespace ElectronicMaps.Infrastructure.Queries
{
    public class EfComponentReadRepository : IComponentReadRepository
    {
        private readonly AppDbContext _db;

        public EfComponentReadRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<bool> ExistsAsync(string name, CancellationToken ct)
        {
            return _db.Components.AsNoTracking().AnyAsync(c => c.Name == name, ct);
        }
        // TODO: Разобраться
        public Task<IReadOnlyList<ComponentLookUpDto>> GetAllAsync(CancellationToken ct)
        {
            return _db.Components.AsNoTracking().OrderBy(c => c.Id)
                .Select(c => new ComponentListItemDto(
                    c.Id,
                    c.Name,
                    c.FormType.Code,
                    c.ComponentFamily != null ? c.ComponentFamily.Name : null
                    )).ToListAsync(ct).ContinueWith(t => (IReadOnlyList<ComponentLookUpDto>)t.Result, ct);
        }

        public Task<IReadOnlyList<ComponentLookUpDto>> GetByFormCodeAsync(string formCode, CancellationToken ct)
        {
            return _db.Components.AsNoTracking().Where(c => c.FormType.Code == formCode)
                .OrderBy(c => c.Id)
                .Select(c => new ComponentListItemDto(
                    c.Id,
                    c.Name,
                    c.FormType.Code,
                    c.ComponentFamily != null ? c.ComponentFamily.Name : null))
                .ToListAsync(ct).ContinueWith(t => (IReadOnlyList<ComponentLookUpDto>)t.Result, ct);
        }

        public Task<ComponentLookUpDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            return _db.Components.AsNoTracking().Where(c => c.Id == id)
                .Select(c => new ComponentLookUpDto(
                c.Id,
                c.Name,
                c.FormTypeId,
                c.FormType.Code,
                c.FormType.DisplayName,
                c.ComponentFamilyId,
                c.ComponentFamily.Name
                )).FirstOrDefaultAsync(ct);
        }

        public Task<ComponentLookUpDto?> GetByNameAsync(string name, CancellationToken ct)
        {
            return _db.Components.AsNoTracking().Where(c => c.Name == name)
                .Select(c => new ComponentLookUpDto(
                    c.Id,
                    c.Name,
                    c.FormTypeId,
                    c.FormType.Code,
                    c.FormType.DisplayName,
                    c.ComponentFamilyId,
                    c.ComponentFamily.Name
                )).FirstOrDefaultAsync(ct);
        }

        public Task<IReadOnlyList<ComponentLookUpDto>> GetLookupByNamesAsync(IEnumerable<string> names, CancellationToken ct)
        {
            return _db.Components.AsNoTracking().Where(c => names.Contains(c.Name))
                .Select(c => new ComponentLookUpDto(
                    c.Id,
                    c.Name,
                    c.FormTypeId,
                    c.FormType.Code,
                    c.FormType.DisplayName,
                    c.ComponentFamilyId,
                    c.ComponentFamily != null ? c.ComponentFamily.Name : null
                    ))
                .ToListAsync(ct).ContinueWith(t => (IReadOnlyList<ComponentLookUpDto>)t.Result, ct);
        }
    }
}
