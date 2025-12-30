using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.OpenXmlWord.Forms.Schemas
{
    public class JsonSchemaStore
    {
        private readonly string _schemasRoot;
        private readonly ConcurrentDictionary<string, FormSchema> _cache = new();

        public JsonSchemaStore(string schemasRoot) 
        {
            _schemasRoot = schemasRoot ?? throw new ArgumentNullException(nameof(schemasRoot));
        }

        public FormSchema Get(string formCode)
        {
            return _cache.GetOrAdd(formCode, Load);
        }

        FormSchema Load(string code)
        {
            var path = Path.Combine(_schemasRoot, "Shemas", $"Form{code}.shema.json");
            if(!File.Exists(path))
                throw new FileNotFoundException(path);

            var json = File.ReadAllText(path);
            var schema = JsonSerializer.Deserialize<FormSchema>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            if(schema == null)
                throw new InvalidOperationException($"Failed to deserialize schema: {path}");

            if (!string.Equals(schema.FormCode, code, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Schema FormCode mismatch. Expected {code}, got {schema.FormCode}");

            if (schema.ItemsPerPage <= 0)
                throw new InvalidOperationException($"ItemsPerPage must be > 0 for form {code}");

            return schema;
        }
    }
}
