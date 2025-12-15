using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Remarks;
using ElectronicMaps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Queries
{
    public class EfFamilyRemarkQuery : IFamilyRemarksQuery
    {
        private readonly AppDbContext _db;

        public EfFamilyRemarkQuery(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<RemarkDto>> GetForFamilyAsync(int familyId, CancellationToken ct)
        {
            return await _db.Set<ComponentFamilyRemark>().AsNoTracking()
                .Where(x => x.ComponentFamilyId == familyId && x.Remark.IsActive)
                .OrderBy(x => x.Order)
                .Select(x => new RemarkDto(x.RemarkId, x.Remark.Text, x.Remark.Order))
                .ToListAsync(ct);
        }
    }
}
