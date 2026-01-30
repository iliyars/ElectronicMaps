using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Documents.Models;
using ElectronicMaps.Documents.Services;
using Microsoft.Extensions.Logging;

namespace ElectronicMaps.Application.Abstractions.Services
{
  public class DocumentService : IDocumentService
  {
    private readonly IDocumentGenerator _generator;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IDocumentGenerator generator, ILogger<DocumentService> logger)
    {
      _generator = generator;
      _logger = logger;
    }

    public async Task<string> CreateDocumentAsync(
      IReadOnlyList<ComponentDraft> components,
      string formCode,
      string? outputPath = null)
    {
      _logger.LogInformation(
           "Начало создания документа для {Count} компонентов формы {FormCode}",
           components.Count,
           formCode);

      // ═══════════════════════════════════════════════════════
      // ШАГ 1: Преобразуем ComponentDraft → ComponentData
      // ═══════════════════════════════════════════════════════

      var componentData = components.Select(draft => ConvertToComponentData(draft)).ToList();

      _logger.LogDebug("Преобразовано {Count} компонентов в ComponentData", componentData.Count);


      // ═══════════════════════════════════════════════════════
      // ШАГ 2: Определяем путь для сохранения
      // ═══════════════════════════════════════════════════════
      outputPath ??= GenerateOutputFile(formCode);

      // ═══════════════════════════════════════════════════════
      // ШАГ 3: Создаём запрос на генерацию
      // ═══════════════════════════════════════════════════════
      var request = new DocumentGenerationRequest
      {
        FormCode = formCode,
        Components = componentData,
        OutputPath = outputPath
      };

      // ═══════════════════════════════════════════════════════
      // ШАГ 4: Генерируем документ
      // ═══════════════════════════════════════════════════════

      var result = await _generator.GenerateAsync(request);

      if (!result.Success)
      {
        _logger.LogError("Ошибка генерации документа: {Error}", result.ErrorMessage);
        throw new InvalidOperationException($"Ошибка генерации документа: {result.ErrorMessage}");
      }

      _logger.LogInformation(
            "✓ Документ создан успешно: {Path}",
            result.OutputPath);

      _logger.LogInformation(
          "   Статистика: {Components} компонентов → {Tables} таблиц → {Cells} ячеек",
          result.ProcessedComponents,
          result.CreatedTables,
          result.WrittenCells);

      return result.OutputPath!;
    }

    private ComponentData ConvertToComponentData(ComponentDraft draft)
    {
      var parameters = new Dictionary<string, string>();

      //Базовые поля
      parameters["ComponentName"] = draft.Name ?? "";
      parameters["ComponentDesignation"] = draft.Family;
      parameters["Quantity"] = draft.Quantity.ToString();

      //Парасетры из SchematicParameters (параметры из схемы)
      //TODO: if (draft.Sc)

      //Параметры из НДТ
      if (draft.NdtParametersOverrides != null)
      {
        foreach (var param in draft.NdtParametersOverrides)
        {
          var paramId = param.Key;
          var paramValue = param.Value;

          parameters[$"Param_{paramId}"] = paramValue.StringValue ?? "";
        }
      }

      return ComponentData.FromDictionary(parameters);
    }

    private string GenerateOutputFile(string formCode)
    {
      var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
      var fileName = $"{formCode}_{timestamp}.docx";
      return Path.Combine("output", fileName);
    }
  }
}
