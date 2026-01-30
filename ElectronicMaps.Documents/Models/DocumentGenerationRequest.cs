using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Documents.Models
{
    public class DocumentGenerationRequest
    {
        public required string FormCode { get; init; }

        public required List<ComponentData> Components { get; init; } = new List<ComponentData>();

        public required string OutputPath { get; init; }

        public string? TemplatesDirectory { get; init; }

        public string? SchemasDirectory { get; init; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FormCode))
            {
                throw new ArgumentException("FormCode не может быть пустым", nameof(FormCode));
            }

            if (Components == null || Components.Count == 0)
            {
                throw new ArgumentException("Список компонентов не может быть пустым", nameof(Components));
            }

            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                throw new ArgumentException("OutputPath не может быть пустым", nameof(OutputPath));
            }
        }
    }
}
