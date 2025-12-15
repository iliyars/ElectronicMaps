using ElectronicMaps.Application.DTO.Remarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IComponentRemarksQuery
    {
        Task<IReadOnlyList<RemarkDto>> GetForComponentAsync(int componentId, CancellationToken ct);

    }
}
