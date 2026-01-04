using ElectronicMaps.Application.DTOs.Remarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Queries.Families
{
    public interface IFamilyRemarksQuery
    {
        Task<IReadOnlyList<RemarkDto>> GetForFamilyAsync(int familyId, CancellationToken ct);
    }
}
