using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Repositories
{
    internal class EfComponentFamilyRepository : Domain.Repositories.IComponentFamilyRepository
    {
        private readonly AppDbContext _db;

        public EfComponentFamilyRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ComponentFamily componentFamily, CancellationToken ct)
        {
             await _db.ComponentFamilies.AddAsync(componentFamily, ct);
        }

        public Task<ComponentFamily?> GetByIdAsync(int id, CancellationToken ct)
        {
            return _db.ComponentFamilies.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public Task<ComponentFamily?> GetByNameAsync(string name, CancellationToken ct)
        {
            return _db.ComponentFamilies.FirstOrDefaultAsync(x => x.Name == name, ct);
        }

        public void Remove(ComponentFamily componentFamily)
        {
            _db.ComponentFamilies.Remove(componentFamily);
        }

        public void Update(ComponentFamily componentFamily)
        {
            _db.ComponentFamilies.Update(componentFamily);
        }
    }
}
