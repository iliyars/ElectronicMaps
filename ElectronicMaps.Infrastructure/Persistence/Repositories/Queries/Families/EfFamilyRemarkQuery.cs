using ElectronicMaps.Application.Abstractions.Queries;
using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.DTOs.Remarks;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;


namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Families
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
