using ElectronicMaps.Application.Abstractions.Queries.Forms;
using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Forms
{
    public class EfFormTypeReadRepository : IFormTypeReadRepository
    {
        private readonly AppDbContext _db;

        public EfFormTypeReadRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<IReadOnlyList<FormTypeDto>> GetAllAsync(CancellationToken ct)
        {
            return _db.Set<FormType>().AsNoTracking()
                .OrderBy(x => x.Code)
                .Select(x => new FormTypeDto(x.Id, x.Code, x.DisplayName))
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<FormTypeDto>)t.Result, ct);
        }

        public Task<FormTypeDto?> GetByCodeAsync(string code, CancellationToken ct)
        {
            return _db.Set<FormType>().AsNoTracking()
                .Where(x => x.Code == code)
                .Select(x => new FormTypeDto(x.Id, x.Code, x.DisplayName))
                .FirstOrDefaultAsync(ct);
        }

        public Task<FormTypeDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            return _db.Set<FormType>().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new FormTypeDto(x.Id, x.Code, x.DisplayName))
                .FirstOrDefaultAsync(ct);
        }
    }
}
