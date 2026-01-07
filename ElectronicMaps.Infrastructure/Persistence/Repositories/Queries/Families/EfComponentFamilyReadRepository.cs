using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.DTOs.Families;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Families
{


    /// <summary>
    /// Read-репозиторий для получения минимальной справочной информации
    /// о семействах компонентов.
    ///</summary>
    public class EfComponentFamilyReadRepository : IComponentFamilyReadRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<EfComponentFamilyReadRepository> _logger;

        public EfComponentFamilyReadRepository(
            AppDbContext db,
            ILogger<EfComponentFamilyReadRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<ComponentFamilyLookupDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            var family = await _db.ComponentFamilies
                .Include(f => f.FamilyFormType)
                .Include(f => f.Components)
                .FirstOrDefaultAsync(f => f.Id == id, ct);

            if (family == null)
            {
                return null;
            }

            return new ComponentFamilyLookupDto
            {
                Id = family.Id,
                Name = family.Name,
                ComponentCount = family.Components.Count,
                FormTypeName = family.FamilyFormType?.DisplayName
            };
        }

        public async Task<ComponentFamilyLookupDto?> FindByNameAsync(string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var family = await _db.ComponentFamilies
            .Include(f => f.FamilyFormType)
            .Include(f => f.Components)
            .FirstOrDefaultAsync(f => f.Name == name, ct);

            if (family == null)
                return null;

            return new ComponentFamilyLookupDto
            {
                Id = family.Id,
                Name = family.Name,
                ComponentCount = family.Components.Count,
                FormTypeName = family.FamilyFormType?.DisplayName
            };
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            return await _db.ComponentFamilies.AnyAsync(f => f.Id == id, ct);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _db.ComponentFamilies.AnyAsync(f => f.Name == name, ct);
        }

        public async Task<IReadOnlyList<ComponentFamilyLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            var families = await _db.ComponentFamilies
                .Include(f => f.FamilyFormType)
                .Include(f => f.Components)
                .OrderBy(f => f.Name)
                .ToListAsync();

            return families.Select(f => new ComponentFamilyLookupDto
            {
                Id = f.Id,
                Name = f.Name,
                ComponentCount = f.Components.Count,
                FormTypeName = f.FamilyFormType?.DisplayName
            }).ToList();
            

        }

        public async Task<IReadOnlyList<ComponentFamilyLookupDto>> GetPagedAsync(
            int skip,
            int take,
            CancellationToken ct = default)
        {
            var families = await _db.ComponentFamilies
                .Include(f => f.FamilyFormType)
                .Include(f => f.Components)
                .OrderBy(f => f.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return families.Select(f => new ComponentFamilyLookupDto
            {
                Id = f.Id,
                Name = f.Name,
                ComponentCount = f.Components.Count,
                FormTypeName = f.FamilyFormType?.DisplayName
            }).ToList();
        }

        public async Task<IReadOnlyList<ComponentFamilyLookupDto>> GetByNamesAsync(IEnumerable<string> names, CancellationToken ct = default)
        {
            var namesList = names?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

            if (namesList == null || namesList.Count == 0)
                return Array.Empty<ComponentFamilyLookupDto>();

            _logger.LogDebug("Батч запрос семейств: {Count} имён", namesList.Count);

            var families = await _db.ComponentFamilies
                .Include(f => f.FamilyFormType)
                .Include(f => f.Components)
                .Where(f => namesList.Contains(f.Name))
                .ToListAsync(ct);

            _logger.LogDebug(
                "Найдено семейств: {Found} из {Total}",
                families.Count,
                namesList.Count);

            return families.Select(f => new ComponentFamilyLookupDto
            {
                Id = f.Id,
                Name = f.Name,
                ComponentCount = f.Components.Count,
                FormTypeName = f.FamilyFormType?.DisplayName,
                FormTypeId = f.FamilyFormTypeId,
                FormCode = f.FamilyFormType?.Code
            }).ToList();
        }
    }
}
