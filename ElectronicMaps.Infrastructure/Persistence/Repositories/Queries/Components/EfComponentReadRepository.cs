using ElectronicMaps.Application.Abstractions.Queries.Components;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Components
{
    public class EfComponentReadRepository : IComponentReadRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<EfComponentReadRepository> _logger;

        public EfComponentReadRepository(
            AppDbContext db,
            ILogger<EfComponentReadRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<ComponentLookupDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var component = await _db.Components
                .Include(c => c.ComponentFamily)
                .Include(c => c.FormType)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (component == null)
                return null;

            return new ComponentLookupDto
            {
                Id = component.Id,
                Name = component.Name,
                FamilyId = component.ComponentFamilyId,
                FamilyName = component.ComponentFamily?.Name,
                FormTypeId = component.FormTypeId,
                FormTypeName = component.FormType?.DisplayName,
                FormCode = component.FormType?.Code
            };
        }

        public async Task<ComponentLookupDto?> FindByNameAsync(string name, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var component = await _db.Components
                .Include(c => c.ComponentFamily)
                .Include(c => c.FormType)
                .FirstOrDefaultAsync(c => c.Name == name, ct);

            if (component == null)
                return null;

            return new ComponentLookupDto
            {
                Id = component.Id,
                Name = component.Name,
                FamilyId = component.ComponentFamilyId,
                FamilyName = component.ComponentFamily?.Name,
                FormTypeId = component.FormTypeId,
                FormTypeName = component.FormType?.DisplayName,
                FormCode = component.FormType?.Code
            };
        }

        public async Task<bool> ExistsAsync(string name, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _db.Components.AnyAsync(c => c.Name == name, ct);
        }

        public async Task<IReadOnlyList<ComponentLookupDto>> GetByFamilyIdAsync(
        int familyId,
        CancellationToken ct = default)
        {
            var components = await _db.Components
                .Include(c => c.ComponentFamily)
                .Include(c => c.FormType)
                .Where(c => c.ComponentFamilyId == familyId)
                .OrderBy(c => c.Name)
                .ToListAsync(ct);

            return components.Select(c => new ComponentLookupDto
            {
                Id = c.Id,
                Name = c.Name,
                FamilyId = c.ComponentFamilyId,
                FamilyName = c.ComponentFamily?.Name,
                FormTypeId = c.FormTypeId,
                FormTypeName = c.FormType?.DisplayName,
                FormCode = c.FormType?.Code
            }).ToList();
        }

        public async Task<IReadOnlyList<ComponentLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            var components = await _db.Components
                .Include(c => c.ComponentFamily)
                .Include(c => c.FormType)
                .OrderBy(c => c.Name)
                .ToListAsync(ct);

            return components.Select(c => new ComponentLookupDto
            {
                Id = c.Id,
                Name = c.Name,
                FamilyId = c.ComponentFamilyId,
                FamilyName = c.ComponentFamily?.Name,
                FormTypeId = c.FormTypeId,
                FormTypeName = c.FormType?.DisplayName,
                FormCode = c.FormType?.Code
            }).ToList();
        }
        /// <summary>
        /// Батч запрос: получить компоненты по списку имён
        /// </summary>
        public async Task<IReadOnlyList<ComponentLookupDto>> GetByNamesAsync(
            IEnumerable<string> names,
            CancellationToken ct = default)
        {
            var namesList = names?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

            if (namesList == null || namesList.Count == 0)
                return Array.Empty<ComponentLookupDto>();

            _logger.LogDebug("Батч запрос компонентов: {Count} имён", namesList.Count);

            var components = await _db.Components
                .Include(c => c.ComponentFamily)
                .Include(c => c.FormType)
                .Where(c => namesList.Contains(c.Name))
                .ToListAsync(ct);

            _logger.LogDebug(
                "Найдено компонентов: {Found} из {Total}",
                components.Count,
                namesList.Count);

            return components.Select(c => new ComponentLookupDto
            {
                Id = c.Id,
                Name = c.Name,
                FamilyId = c.ComponentFamilyId,
                FamilyName = c.ComponentFamily?.Name,
                FormTypeId = c.FormTypeId,
                FormTypeName = c.FormType?.DisplayName,
                FormCode = c.FormType?.Code
            }).ToList();
        }
    }
}
