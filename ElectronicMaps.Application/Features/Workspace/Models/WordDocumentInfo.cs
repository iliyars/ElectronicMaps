using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Features.Workspace.Models
{
    public record WordDocumentInfo(
        Guid DocumentId,

        string Name,

        string FormCode,

        IReadOnlyList<Guid> ComponentIds,

         DateTimeOffset CreatedAtUtc
        );
    
}
