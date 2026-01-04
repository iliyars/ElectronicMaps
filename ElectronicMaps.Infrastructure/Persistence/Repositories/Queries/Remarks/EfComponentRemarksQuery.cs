using ElectronicMaps.Application.Abstractions.Queries;
using ElectronicMaps.Application.Abstractions.Queries.Remarks;
using ElectronicMaps.Application.DTOs.Remarks;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Remarks
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
