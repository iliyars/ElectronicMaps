using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Queries
{
    public class EfParameterValueReadRepository : IParameterValueReadRepository
    {
        private readonly AppDbContext _db;

        public EfParameterValueReadRepository(AppDbContext db) => _db = db;


        public Task<IReadOnlyList<ParameterValueDto>> GetComponentValuesAsync(int componentId, CancellationToken ct)
        {
            return _db.Set<ParameterValue>().AsNoTracking()
                .Where(v => v.ComponentId == componentId)
                .Select(v => new ParameterValueDto(
                    v.ParameterDefinitionId,
                    v.ParameterDefinition.Code,
                    v.StringValue,
                    v.DoubleValue,
                    v.IntValue,
                    v.Pins
                    ))
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<ParameterValueDto>)t.Result, ct);
        }

        public Task<IReadOnlyList<ParameterValueDto>> GetFamilyValuesAsync(int familyId, CancellationToken ct)
        {
            return _db.Set<ParameterValue>().AsNoTracking()
                .Where(v => v.ComponentFamilyId == familyId)
                .Select(v => new ParameterValueDto(
                    v.ParameterDefinitionId,
                    v.ParameterDefinition.Code,
                    v.StringValue,
                    v.DoubleValue,
                    v.IntValue,
                    v.Pins
                    ))
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<ParameterValueDto>)t.Result, ct);
        }
    }
}
