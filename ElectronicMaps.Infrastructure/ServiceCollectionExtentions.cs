using ElectronicMaps.Application.Abstractions.Queries.Workspace;
using ElectronicMaps.Application.Abstractions.Commands;
using ElectronicMaps.Domain.Repositories;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectronicMaps.Infrastructure.Persistence;
using ElectronicMaps.Application.Abstractions.Queries.Components;
using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.Abstractions.Queries.Forms;
using ElectronicMaps.Application.Abstractions.Queries.Parameters;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using ElectronicMaps.Infrastructure.Persistence.Initialization;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Commands;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Components;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Families;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Forms;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Parameters;
using ElectronicMaps.Application.Abstractions.Persistence;

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
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // Write repositories



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
