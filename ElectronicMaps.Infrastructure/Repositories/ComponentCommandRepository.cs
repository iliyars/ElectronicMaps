using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Repositories
{
    public class ComponentCommandRepository : IComponentCommandRepository
    {

        private readonly AppDbContext _dbContext;

        public ComponentCommandRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task AddAsync(Domain.Entities.Component component, CancellationToken ct) =>
            _dbContext.Components.AddAsync(component, ct).AsTask();


        public Task AddFamilyAsync(Domain.Entities.ComponentFamily family, CancellationToken ct)
        {
            _dbContext.ComponentFamilies.Add(family);
            return Task.CompletedTask;
        }

        public Task AddParameterValuesAsync(IEnumerable<ParameterValue> values, CancellationToken ct)
        {
            _dbContext.ParameterValues.AddRange(values);
            return Task.CompletedTask;
        }


        public Task SaveChangesAsync(CancellationToken ct) =>
            _dbContext.SaveChangesAsync(ct);

    }
}
