using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Repositories
{
    public class AppUserRepository : IAppUserRepository
    {

        private readonly AppDbContext _dbContext;

        public AppUserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task AddAsync(AppUser user, CancellationToken ct = default)
        {
            return _dbContext.AddAsync(user, ct).AsTask();
        }

        public async Task<IReadOnlyList<AppUser>> GetActiveAsync(CancellationToken ct = default)
        {
           var list = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.IsActive)
                .OrderBy(u => u.DisplayName)
                .ToListAsync(ct);
            return list;
        }

        public async Task<IReadOnlyList<AppUser>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
        {
            var query = _dbContext.Users.AsNoTracking().AsQueryable();

            if(!includeInactive)
            {
                query = query.Where(u => u.IsActive);
            }

            var list = await query
                .OrderBy(u => u.DisplayName)
                .ToListAsync(ct);

            return list;
        }

        public async Task<AppUser?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<AppUser?> GetByWindowsIdentityAsync(string windowsIdentity, CancellationToken ct = default)
        {
           if(string.IsNullOrWhiteSpace(windowsIdentity))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(windowsIdentity));
            }
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.WindowsIdentity == windowsIdentity, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default) =>
            _dbContext.SaveChangesAsync(ct);
    }
}
