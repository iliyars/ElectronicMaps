using ElectronicMaps.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Stores
{
    public class JsonComponentStoreSerializer : IComponentStoreSerializer
    {
        private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        

        public Task SerializeAsync(
            Stream stream,
            IReadOnlyDictionary<string, List<AnalyzedComponentDto>> map,
            CancellationToken ct = default)
        {
            return JsonSerializer.SerializeAsync(stream, map, _options, ct);
        }

        public async Task<Dictionary<string, List<AnalyzedComponentDto>>> DeserializeAsync(Stream stream, CancellationToken ct = default)
        {
            var result = await JsonSerializer.DeserializeAsync<Dictionary<string, List<AnalyzedComponentDto>>>(
                stream, _options, ct) ?? new Dictionary<string, List<AnalyzedComponentDto>>();

            return result ?? new();
        }

    }
}
