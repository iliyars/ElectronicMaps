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
    public class EfComponentRemarksQuery : IComponentRemarksQuery
    {
        private readonly AppDbContext _db;

        public EfComponentRemarksQuery(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<RemarkDto>> GetForComponentAsync(int componentId, CancellationToken ct)
        {
            return await _db.Set<ComponentRemark>().AsNoTracking()
                .Where(x => x.ComponentId == componentId && x.Remark.IsActive)
                .OrderBy(x => x.Order)
                .Select(x => new RemarkDto(
                    x.RemarkId,
                    x.Remark.Text,
                    x.Order))
                .ToListAsync(ct);
        }
    }
}
