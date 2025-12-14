using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.WorkspaceProject
{
    public interface IWorkspaceProjectSerializer
    {
        Task SaveAsync(string filePath, WorkspaceProject.Models.WorkspaceProject project, IReadOnlyCollection<WordDocumentBinary> docs, CancellationToken ct);
        Task<WorkspaceProjectLoadResult> LoadAsync(string filePath, CancellationToken ct);
    }

    public record WordDocumentBinary(Guid DocumentId, byte[] Content);

    public record WorkspaceProjectLoadResult(
    WorkspaceProject.Models.WorkspaceProject Project,
    IReadOnlyList<WordDocumentBinary> Documents
);

}
