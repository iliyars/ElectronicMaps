using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ElectronicMaps.Documents.Models
{
    public class TemplateSchema
    {
        [JsonPropertyName("templateCode")]
        public required string TemplateCode { get; init; }

        [JsonPropertyName("fileName")]
        public required string TemplateName { get; init; }

        [JsonPropertyName("componentsPerTable")]
        public int ComponentsPerTable { get; init; }

        [JsonPropertyName("columnOffset")]
        public int ColumnOffset { get; init; }

        [JsonPropertyName("fieldMappings")]
        public required List<FieldMapping> FieldMappings { get; init; }
    }
}

public sealed class FieldMapping
{
    [JsonPropertyName("fieldCode")]
    public required string FieldCode { get; init; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    [JsonPropertyName("column")]
    public required int Column { get; init; }
    [JsonPropertyName("row")]
    public required int Row { get; init; }
    [JsonPropertyName("isHeader")]
    public bool IsHeader { get; init; } = false;
    [JsonPropertyName("isTableHeader")]
    public bool IsTableHeader { get; init; } = false;

    public int GetColumnForComponent(int componentIndex, int columnOffset)
    {
        //TODO: сделать различие между header и обычными полями
        return Column + (componentIndex * columnOffset);
    }

    public override string ToString() =>
       $"{FieldCode} → [{Row}, {Column}]";
}
