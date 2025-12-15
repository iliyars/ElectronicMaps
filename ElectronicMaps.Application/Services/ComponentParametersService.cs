using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    ///<inheritdoc/>
    public class ComponentParametersService : IComponentParametersService
    {

        

        public ComponentParametersService()
        {
            
        }

        ///<inheritdoc/>
        public async Task<IReadOnlyCollection<ParameterDto>> GetComponentParametersAsync(int componentId, CancellationToken ct)
        {
            //var component =  await _query.GetComponentByIdWithFormAsync(componentId, ct);

            //if(component is null) 
            //    return Array.Empty<ParameterDto>();

            //var definitions = component.FormType.Parameters;

            //var values = await _query.GetParameterValuesAsync(componentId, ct);

            //return BuildParameterDtos(definitions, values);
            throw new NotImplementedException();



        }
        ///<inheritdoc/>
        public async Task<IReadOnlyCollection<ParameterDto>> GetFamilyParametersAsync(int familyId, CancellationToken ct)
        {
            //var family = await _query.GetFamilyByIdWithFormAsync(familyId, ct) 
            //    ?? throw new InvalidOperationException($"Family {familyId} not found");

            //if (family.FamilyFormType is null)
            //    return Array.Empty<ParameterDto>();

            //var definitions = family.FamilyFormType.Parameters;

            //var values = await _query.GetFamilyParameterValuesAsync(familyId, ct);

            //return BuildParameterDtos(definitions, values);
            throw new NotImplementedException();
        }

        private static IReadOnlyList<ParameterDto> BuildParameterDtos(
            IEnumerable<ParameterDefinition> definitions,
            IReadOnlyList<ParameterValue> values)
        {
            var valuesByDefId = values.ToDictionary(v => v.ParameterDefinitionId);
            var list = new List<ParameterDto>();

            foreach (var def in definitions.OrderBy(d => d.Order))
            {
                valuesByDefId.TryGetValue(def.Id, out var v);

                list.Add(new ParameterDto
                {
                    Code = def.Code,
                    DisplayName = def.DisplayName,
                    Unit = def.Unit,
                    StringValue = v?.StringValue,
                    DoubleValue = v?.DoubleValue,
                    IntValue = v?.IntValue,
                    Pins = v?.Pins
                });
            }

            return list;
        }
    }
}
