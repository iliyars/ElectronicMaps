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
    public class EfComponentRepository : IComponentRepository
    {
        private readonly AppDbContext _db;

        public EfComponentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Component component, CancellationToken cancellationToken)
        {
            await _db.Components.AddAsync(component, cancellationToken);
        }

        public Task<Component?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return _db.Components.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public Task<Component?> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            return _db.Components.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
        }

        public void Remove(Component component)
        {
            _db.Components.Remove(component);
        }

        public void Update(Component component)
        {
            _db.Components.Update(component);
        }
    }
}
