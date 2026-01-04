using ElectronicMaps.Application.Abstractions.Queries;
using ElectronicMaps.Application.Abstractions.Queries.Parameters;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;


namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Parameters
{
    public class EfParameterDefinitionReadRepository : IParameterDefinitionReadRepository
    {

        private readonly AppDbContext _db;

        public EfParameterDefinitionReadRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetByFormTypeIdAsync(int formTypeId, CancellationToken ct)
        {
            return await _db.Set<ParameterDefinition>()
                .AsNoTracking()
                .Where(p => p.FormTypeId == formTypeId)
                .OrderBy(p => p.Order)
                .Select(p => new ParameterDefinitionDto(
                    p.Id,
                    p.Code,
                    p.DisplayName,
                    p.ValueKind,
                    p.Order,
                    p.Unit))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetByFormCodeAsync(string formCode, CancellationToken ct)
        {
            return await _db.Set<ParameterDefinition>()
                 .AsNoTracking()
                 .Where(p => p.FormType.Code == formCode)
                 .OrderBy(p => p.Order)
                 .Select(p => new ParameterDefinitionDto(
                     p.Id,
                     p.Code,
                     p.DisplayName,
                     p.ValueKind,
                     p.Order,
                     p.Unit))
                 .ToListAsync(ct);
        }
    }
}
