using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Families;
using ElectronicMaps.Application.DTO.Forms;
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
    public class EfFamilyDetailsQuery : IFamilyDetailsQuery
    {
        private readonly AppDbContext _db;
        public EfFamilyDetailsQuery(AppDbContext db)
        {
            _db = db;
        }

        public async Task<FamilyDetailsDto?> GetAsync(int familyId, CancellationToken ct)
        {
            var family = await _db.ComponentFamilies.AsNoTracking()
                .Where(f => f.Id == familyId)
                .Select(f => new
                {
                   f.Id,
                   f.Name,
                   f.FamilyFormTypeId,
                   f.VerificationStatus,
                   Form = new FormTypeDto(f.FamilyFormTypeId, f.FamilyFormType!.Code, f.FamilyFormType!.DisplayName),
                   ComponentsCount = f.Components.Count

                })
                .FirstOrDefaultAsync(ct);

            if (family is null)
            {
                return null;
            }

            var defs = await _db.Set<ParameterDefinition>().AsNoTracking()
                .Where(p => p.FormTypeId == family.FamilyFormTypeId)
                .OrderBy(p => p.Order)
                .Select(p => new ParameterDefinitionDto(
                    p.Id,
                    p.Code,
                    p.DisplayName,
                    p.ValueKind,
                    p.Order,
                    p.Unit))
                .ToListAsync(ct);


            var values = await _db.Set<ParameterValue>().AsNoTracking()
                .Where(v => v.ComponentFamilyId == familyId && v.ComponentId == null)
                .OrderBy(v => v.ParameterDefinition.Order)
                .Select(v => new ParameterValueDto(
                    v.ParameterDefinitionId,
                    v.ParameterDefinition.Code,
                    v.StringValue,
                    v.DoubleValue,
                    v.IntValue,
                    v.Pins))
                .ToListAsync(ct);

            return new FamilyDetailsDto(
                family.Id,
                family.Name,
                family.Form,
                family.VerificationStatus,
                family.ComponentsCount,
                defs,
                values);

        }
    }
}
