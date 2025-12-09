using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Repositories;
using ElectronicMaps.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public class FormQueryService : IFormQueryService
    {

        private readonly IComponentQueryRepository _repo;

        public FormQueryService(IComponentQueryRepository repo)
        {
            _repo = repo;
        }

        public async Task<IReadOnlyList<ParameterDto>> GetComponentFormAsync(string componentName, CancellationToken ct = default)
        {
            var component = await _repo.GetByNameAsync(componentName, ct);
            if (component is null)
                return Array.Empty<ParameterDto>();

            var formType = await _repo.GetFormTypeByCodeAsync(component.FormCode, ct);
            if (formType is null)
                return Array.Empty<ParameterDto>();

            var values = await _repo.GetParameterValuesAsync(component.Id, ct);

            var result = new List<ParameterDto>();


            foreach (var def in formType.Parameters.OrderBy(p => p.Order))
            {
                var value = values.FirstOrDefault(v => v.ParameterDefinitionId == def.Id);

                result.Add(new ParameterDto
                {
                    Code = def.Code,
                    DisplayName = def.DisplayName,
                    Unit = def.Unit,
                    StringValue = value?.StringValue,
                    DoubleValue = value?.DoubleValue,
                    IntValue = value?.IntValue,
                    Pins = value?.Pins
                });
            }

            return result;
        }

        public Task<bool> ComponentExistsAsync(string componentName, CancellationToken ct = default) => _repo.ExistsAsync(componentName, ct);
    }
}
