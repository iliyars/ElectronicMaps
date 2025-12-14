using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Queries
{
    public class EfParameterDefinitionReadRepository : IParameterDefinitionReadRepository
    {

        private readonly AppDbContext _db;

        public EfParameterDefinitionReadRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<IReadOnlyList<ParameterDefinitionDto>> GetByFormTypeIdAsync(int formTypeId, CancellationToken ct)
        {
            return _db.Set<ParameterDefinition>().AsNoTracking()
                .Where(x => x.FormTypeId == formTypeId)
                .OrderBy(p => p.Order)
                .Select(p => new ParameterDefinitionDto(
                    p.Id,
                    p.Code,
                    p.DisplayName,
                    p.ValueKind,
                    p.Order,
                    p.Unit
                    ))
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<ParameterDefinitionDto>)t.Result, ct);

        }

        public Task<IReadOnlyList<ParameterDefinitionDto>> GetByFormCodeAsync(string formCode, CancellationToken ct)
        {
            return _db.Set<ParameterDefinition>().AsNoTracking()
                .Where(p => p.FormType.Code == formCode)
                .OrderBy(p => p.Order)
                .Select(p => new ParameterDefinitionDto(
                    p.Id,
                    p.Code,
                    p.DisplayName,
                    p.ValueKind,
                    p.Order,
                    p.Unit
                    ))
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<ParameterDefinitionDto>)t.Result, ct);
        }
    }
}
