using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.DTOs.Families;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;


namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Families
{


    /// <summary>
    /// Read-репозиторий для получения минимальной справочной информации
    /// о семействах компонентов.
    ///</summary>
    public class EfComponentFamilyReadRepository : IComponentFamilyReadRepository
    {
        private readonly AppDbContext _db;

        public EfComponentFamilyReadRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<ComponentFamilyLookupDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            return _db.ComponentFamilies.AsNoTracking().Where(f => f.Id == id).Select(f => new ComponentFamilyLookupDto(
                f.Id,
                f.Name,
                f.FamilyFormTypeId,
                f.FamilyFormType.Code,
                f.VerificationStatus
                ))
                .FirstOrDefaultAsync(ct);
        }

        public Task<ComponentFamilyLookupDto?> GetByNameAsync(string name, CancellationToken ct)
        {
            return _db.ComponentFamilies.AsNoTracking().Where(f => f.Name == name)
                .Select(f => new ComponentFamilyLookupDto(
                    f.Id,
                    f.Name,
                    f.FamilyFormTypeId,
                    f.FamilyFormType.Code,
                    f.VerificationStatus
                    ))
                .FirstOrDefaultAsync(ct);
        }

        public Task<IReadOnlyList<ComponentFamilyLookupDto>> GetLookupByNamesAsync(IEnumerable<string> names, CancellationToken ct)
        {
            var arr = names.Distinct().ToArray();
            return _db.ComponentFamilies.AsNoTracking()
                .Where(f => arr.Contains(f.Name))
                .Select(f => new ComponentFamilyLookupDto(
                    f.Id,
                    f.Name,
                    f.FamilyFormTypeId,
                    f.FamilyFormType.Code,
                    f.VerificationStatus
                    ))
                .ToListAsync(ct).ContinueWith(t => (IReadOnlyList<ComponentFamilyLookupDto>)t.Result, ct);
        }
    }
}
