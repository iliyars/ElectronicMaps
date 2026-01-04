using ElectronicMaps.Application.Abstractions.Queries.Components;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Components
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
        public async Task<IReadOnlyList<ComponentLookUpDto>> GetAllAsync(CancellationToken ct)
        {
            return await _db.Components.AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new ComponentLookUpDto(
                    c.Id,
                    c.Name,

                    c.FormTypeId,
                    c.FormType.Code,
                    c.FormType.DisplayName,

                    c.ComponentFamilyId,
                    c.ComponentFamily.Name,

                    c.VerificationStatus)).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<ComponentLookUpDto>> GetByFormCodeAsync(string formCode, CancellationToken ct)
        {
            return await _db.Components.AsNoTracking().Where(c => c.FormType.Code == formCode)
                .OrderBy(c => c.Id)
                .Select(c => new ComponentLookUpDto(
                    c.Id,
                    c.Name,

                    c.FormTypeId,
                    c.FormType.Code,
                    c.FormType.DisplayName,

                    c.ComponentFamilyId,
                    c.ComponentFamily.Name,

                    c.VerificationStatus)).ToListAsync(ct);
        }
        
        public async Task<ComponentLookUpDto?> GetByIdAsync(int id, CancellationToken ct)
        {
                 return await _db.Components.AsNoTracking().Where(c => c.Id == id)
                .Select(c => new ComponentLookUpDto(
                    c.Id,
                    c.Name,

                    c.FormTypeId,
                    c.FormType.Code,
                    c.FormType.DisplayName,

                    c.ComponentFamilyId,
                    c.ComponentFamily.Name,

                    c.VerificationStatus)).FirstOrDefaultAsync(ct);
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
                    c.ComponentFamily.Name,

                    c.VerificationStatus)).FirstOrDefaultAsync(ct);

        }

        public async Task<IReadOnlyList<ComponentLookUpDto>> GetLookupByNamesAsync(IEnumerable<string> names, CancellationToken ct)
        {
            var arr = names.Distinct().ToArray();

            return await _db.Components.AsNoTracking()
                .Where(c => arr.Contains(c.Name))
                .Select(c => new ComponentLookUpDto
                (
                    c.Id,
                    c.Name,
                    c.FormTypeId,
                    c.FormType.Code,
                    c.FormType.DisplayName,
                    c.ComponentFamilyId,
                    c.ComponentFamily.Name,
                    c.VerificationStatus))
                .ToListAsync(ct);
        }
    }
}
