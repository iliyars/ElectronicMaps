using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Components;
using ElectronicMaps.Application.DTO.Forms;
using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Queries
{
    public class EfComponentDetailsQuery : IComponentDetailsQuery
    {

        private readonly AppDbContext _db;

        public EfComponentDetailsQuery(AppDbContext db) => _db = db;

        public async Task<ComponentDetailsDto> GetAsync(int componentId, CancellationToken ct)
        {

            // Базовая информация о компоненте + форма
            var component = await _db.Components.AsNoTracking()
                .Where(c => c.Id == componentId)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    Form = new FormTypeDto(c.FormType.Id, c.FormType.Code, c.FormType.DisplayName),
                    FormTypeId = c.FormType.Id,
                })
                .FirstOrDefaultAsync(ct);

            if(component is null) 
                return null!;

            // Определение параметров по FormType
            var defs = await _db.Set<ParameterDefinition>().AsNoTracking()
                .Where(p => p.FormTypeId == component.FormTypeId)
                .OrderBy(p => p.Order)
                .Select(p => new ParameterDefinitionDto(
                    p.Id,
                    p.Code,
                    p.DisplayName,
                    p.ValueKind,
                    p.Order,
                    p.Unit
                    ))
                .ToListAsync(ct);

            // Значения параметров по компоненту
            var values = await _db.Set<ParameterValue>().AsNoTracking()
                .Where(v => v.ComponentId == componentId)
                .Select(v => new ParameterValueDto(
                    v.ParameterDefinitionId,
                    v.ParameterDefinition.Code,
                    v.StringValue,
                    v.DoubleValue,
                    v.IntValue,
                    v.Pins))
                .ToListAsync(ct);

            return new ComponentDetailsDto(
                component.Id,
                component.Name,
                component.Form,
                defs,
                values
                );
        }
    }
}
