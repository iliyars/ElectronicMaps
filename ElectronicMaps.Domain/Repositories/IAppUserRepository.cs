using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Repositories
{
    public interface IAppUserRepository
    {
        /// <summary>Returns a user by Id or null if not found.</summary>
        Task<AppUser?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>Returns a user by Windows identity (e.g. "DOMAIN\\user") or null.</summary>
        Task<AppUser?> GetByWindowsIdentityAsync(string windowsIdentity, CancellationToken ct = default);

        /// <summary>Returns all users (optionally including inactive ones).</summary>
        Task<IReadOnlyList<AppUser>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);

        /// <summary>Returns only active users.</summary>
        Task<IReadOnlyList<AppUser>> GetActiveAsync(CancellationToken ct = default);

        /// <summary>Adds a new user to the context (does not save changes).</summary>
        Task AddAsync(AppUser user, CancellationToken ct = default);

        /// <summary>Persists all pending changes to the database.</summary>
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
