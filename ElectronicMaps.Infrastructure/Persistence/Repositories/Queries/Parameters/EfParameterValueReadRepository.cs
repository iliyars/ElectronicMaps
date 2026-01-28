using ElectronicMaps.Application.Abstractions.Queries.Parameters;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Parameters
{
    public class EfParameterValueReadRepository : IParameterValueReadRepository
    {
        private readonly AppDbContext _db;

        public EfParameterValueReadRepository(AppDbContext db) => _db = db;


        public async Task<IReadOnlyList<ParameterValueDto>> GetComponentValuesAsync(int componentId, CancellationToken ct)
        {
            var list =  await _db.Set<ParameterValue>().AsNoTracking()
                .Where(v => v.ComponentId == componentId)
                .Select(v => new ParameterValueDto(
                    v.ParameterDefinitionId,
                    v.ParameterDefinition.Code,
                    v.StringValue,
                    v.DoubleValue,
                    v.IntValue,
                    v.Pins
                    ))
                .ToListAsync(ct);

            return list;
        }

        public async Task<IReadOnlyList<ParameterValueDto>> GetFamilyValuesAsync(int familyId, CancellationToken ct)
        {
            var list = await _db.Set<ParameterValue>().AsNoTracking()
                .Where(v => v.ComponentFamilyId == familyId)
                .Select(v => new ParameterValueDto(
                    v.ParameterDefinitionId,
                    v.ParameterDefinition.Code,
                    v.StringValue,
                    v.DoubleValue,
                    v.IntValue,
                    v.Pins
                    ))
                .ToListAsync(ct);

            return list;
        }
    }
}
