using ElectronicMaps.Application.DTOs.Remarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Queries.Remarks
{
    public interface IComponentRemarksQuery
    {
        Task<IReadOnlyList<RemarkDto>> GetForComponentAsync(int componentId, CancellationToken ct);

    }
}
