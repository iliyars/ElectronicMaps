using ElectronicMaps.Application.DTO.Remarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IFamilyRemarksQuery
    {
        Task<IReadOnlyList<RemarkDto>> GetForFamilyAsync(int familyId, CancellationToken ct);
    }
}
