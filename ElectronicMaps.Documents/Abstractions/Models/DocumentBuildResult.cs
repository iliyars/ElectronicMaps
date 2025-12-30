using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Abstractions.Models
{
    public sealed record DocumentBuildResult(
        byte[] Docx,
        IReadOnlyList<DocumentIssue> Issues);


    public enum DocumentIssueSeverity { Info, Warning, Error }

    public sealed record DocumentIssue(
    DocumentIssueSeverity Severity,
    string Code,
    string Message
);
}
