using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Documents.Models;

namespace ElectronicMaps.Application.Abstractions.Services
{
  /// <summary>
  /// Сервис для генерации Word документов
  /// </summary>
  public interface IDocumentService
  {
    Task<string> CreateDocumentAsync(
      IReadOnlyList<ComponentDraft> components,
      string formCode,
      string? outputPath = null);
  }
}
