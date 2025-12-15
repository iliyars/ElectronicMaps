using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Queries
{
    public class EfWorkspaceQuery : IWorkspaceQuery
    {
        private readonly AppDbContext _db;

        public EfWorkspaceQuery(AppDbContext db) => _db = db;

        public Task<IReadOnlyList<ComponentListItemDto>> GetAllComponentsAsync(CancellationToken ct)
        {
            return _db.Components.AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(c => new ComponentListItemDto(
                    c.Id,
                    c.Name,
                    c.FormType.Code,
                    c.ComponentFamily != null ? c.ComponentFamily.Name : null))
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<ComponentListItemDto>)t.Result, ct);
        }

        public Task<IReadOnlyList<ComponentListItemDto>> GetComponentsByFormAsync(string formCode, CancellationToken ct)
        {
            return _db.Components.AsNoTracking()
                .Where(c => c.FormType.Code == formCode)
                .OrderBy(c => c.Id)
                .Select(c => new ComponentListItemDto(
                    c.Id,
                    c.Name,
                    c.FormType.Code,
                    c.ComponentFamily != null ? c.ComponentFamily.Name : null
                    ))
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<ComponentListItemDto>)t.Result, ct);
        }
    }
}
