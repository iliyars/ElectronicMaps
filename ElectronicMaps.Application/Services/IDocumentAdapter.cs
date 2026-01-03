using ElectronicMaps.Documents.Core.Models;
using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Services
{
    public interface IDocumentAdapter
    {
        Task<IReadOnlyCollection<DocumentItem>> ConvertToDocumentItemsAsync(
            IEnumerable<Component> components,
            CancellationToken ct = default);
    }
}
