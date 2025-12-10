using ElectronicMaps.Application.DTO;
using ElectronicMaps.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public interface IComponentImportService
    {
        Task<Component> EnsureComponentAsync(
            AnalyzedComponentDto dto,
            CancellationToken ct);
    }


    public class ComponentImportService : IComponentImportService
    {
        private readonly IComponentQueryRepository _query;
        private readonly IComponentCommandRepository _command;

        public ComponentImportService(IComponentQueryRepository query, IComponentCommandRepository command)
        {
            _query = query;
            _command = command;
        }

        public async Task<Component> EnsureComponentAsync(AnalyzedComponentDto dto, CancellationToken ct)
        {
            var family = await _query.GetFamilyByNameAsync(dto.Family, ct);

            if(family is null)
            {
                family = new Domain.Entities.ComponentFamily
                {
                    Name = dto.ResolvedFamily,
                    FamilyFormCode = dto.ResolvedFormCode // "FORM_4"s
                };

                await _command.AddFamilyAsync(family, ct);
                await _command.SaveChangesAsync(ct);
            }
        }
    }
}
