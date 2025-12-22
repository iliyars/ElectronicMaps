using ElectronicMaps.Application.Abstractons.Commands;
using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.Services;
using ElectronicMaps.Domain.Repositories;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.Infrastructure.Commands;
using ElectronicMaps.Infrastructure.Initialization;
using ElectronicMaps.Infrastructure.Persistance;
using ElectronicMaps.Infrastructure.Queries;
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


            services.AddSingleton<IConfiguration>(configuration);

            services.AddDbContext<AppDbContext>(options =>
            {
                var provider = DbConfig.GetProvider(configuration);
                var connectionString
                = DbConfig.BuildConnectionString(configuration);

                if(provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    //options.UseSqlServer(connectionString);
                }
                else
                {
                    options.UseSqlite(connectionString);
                }
            });

            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // Write repositories
            services.AddScoped<IComponentRepository, EfComponentRepository>();
            services.AddScoped<IComponentFamilyRepository, EfComponentFamilyRepository>();


            services.AddScoped<IComponentReadRepository, EfComponentReadRepository>();
            services.AddScoped<IComponentFamilyReadRepository, EfComponentFamilyReadRepository>();
            services.AddScoped<IFormTypeReadRepository, EfFormTypeReadRepository>();
            services.AddScoped<IParameterDefinitionReadRepository, EfParameterDefinitionReadRepository>();
            services.AddScoped<IParameterValueReadRepository, EfParameterValueReadRepository>();

            services.AddScoped<IWorkspaceQuery, EfWorkspaceQuery>();
            services.AddScoped<IComponentDetailsQuery, EfComponentDetailsQuery>();
            services.AddScoped<IFamilyDetailsQuery, EfFamilyDetailsQuery>();


            services.AddSingleton<IComponentNameParser, ComponentNameParser>();
            services.AddScoped<IComponentSourceReader, AvsXmlComponentSourceReader>();

            services.AddScoped<ISaveComponent, EfSaveComponent>();

            return services;
        }



    }
}
