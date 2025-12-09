using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//TODO: сделать ExistsAsync батчем (список имён → список найденных)

//TODO:сделать параллельное выполнение (но аккуратно)

//TODO:кешировать результаты на уровне сервиса

//TODO:заменить уникальный словарь на ToLookup с группировкой

namespace ElectronicMaps.Application.Services
{
    /// <summary>
    /// Builds forms for a batch of components using IFormQueryService.
    /// </summary>
    public class ComponentFormBatchService : IComponentFormBatchService
    {

        private readonly IFormQueryService _formQueryService;

        public ComponentFormBatchService(IFormQueryService formQueryService)
        {
            _formQueryService = formQueryService;
        }
        /// <inheritdoc/>
        public async Task<IReadOnlyList<ComponentFormResultDto>> BuildFormsAsync(IReadOnlyList<string> componentNames, CancellationToken ct = default)
        {
            if(componentNames == null || componentNames.Count == 0)
            {
                return Array.Empty<ComponentFormResultDto>();
            }

            var normalized = componentNames
                .Select(name => name?.Trim() ?? string.Empty)
                .ToList();

            // 1. Remove duplicates to avoid repeated DB queries
            var uniqueNames = componentNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var cache = new Dictionary<string, ComponentFormResultDto>(StringComparer.OrdinalIgnoreCase);

            // 2. Query DB once per unique name
            foreach(var name in uniqueNames)
            {

                bool exists = await _formQueryService.ComponentExistsAsync(name, ct);

                if(!exists)
                {
                    cache[name] = new ComponentFormResultDto
                    {
                        ComponentName = name,
                        Found = false,
                        Parameters = Array.Empty<ParameterDto>()
                    };
                    continue;
                }

                var parameters = await _formQueryService.GetComponentFormAsync(name, ct);

                cache[name] = new ComponentFormResultDto
                {
                    ComponentName = name,
                    Found = parameters.Count > 0,
                    Parameters = parameters
                };
            }

            // 3. Build result in the original order (with duplicates if any)
            var result = new List<ComponentFormResultDto>(componentNames.Count);

            foreach (var name in componentNames)
            {
               if(string.IsNullOrWhiteSpace(name))
               {
                    result.Add(new ComponentFormResultDto
                    {
                        ComponentName =  string.Empty,
                        Found = false,
                        Parameters = Array.Empty<ParameterDto>()
                    });

                    continue;
                }

               if(cache.TryGetValue(name, out var dto))
               {
                    result.Add(new ComponentFormResultDto
                    {
                        ComponentName = dto.ComponentName,
                        Found = dto.Found,
                        Parameters = dto.Parameters
                    });
               }
               else
               {
                    result.Add(new ComponentFormResultDto
                    {
                        ComponentName = name,
                        Found = false,
                        Parameters = Array.Empty<ParameterDto>()
                    });
               }
            }

            return result;
        }
    }
}
