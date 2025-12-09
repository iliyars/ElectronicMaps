using ElectronicMaps.Domain.Repositories;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.Infrastructure.Repositories;
using ElectronicMaps.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddMapsInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("AppDb")
           ?? throw new InvalidOperationException("Connection string 'AppDb' not found.");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });


            services.AddScoped<IComponentQueryRepository, ComponentQueryRepository>();
            services.AddScoped<IComponentCommandRepository, ComponentCommandRepository>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();

            services.AddSingleton<IComponentNameParser, ComponentNameParser>();
            services.AddScoped<IComponentSourceReader, AvsXmlComponentSourceReader>();

            return services;
        }



    }
}
