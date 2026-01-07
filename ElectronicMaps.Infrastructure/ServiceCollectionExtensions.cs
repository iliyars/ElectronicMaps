using DocumentFormat.OpenXml.Wordprocessing;
using ElectronicMaps.Application.Abstractions.Commands;
using ElectronicMaps.Application.Abstractions.Persistence;
using ElectronicMaps.Application.Abstractions.Queries.Components;
using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.Abstractions.Queries.Forms;
using ElectronicMaps.Application.Abstractions.Queries.Parameters;
using ElectronicMaps.Application.Abstractions.Queries.Workspace;
using ElectronicMaps.Application.Abstractions.Services;
using ElectronicMaps.Domain.Repositories;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.Infrastructure.Persistence;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using ElectronicMaps.Infrastructure.Persistence.Initialization;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Commands;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Components;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Families;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Forms;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Parameters;
using ElectronicMaps.Infrastructure.Persistence.Repositories.Queries.Workspace;
using ElectronicMaps.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMapsInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AppDbContext>(options =>
            {
                var provider = DbConfig.GetProvider(configuration);
                var connectionString = DbConfig.BuildConnectionString(configuration);

                if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseSqlServer(connectionString);
                }
                else
                {
                    options.UseSqlite(connectionString);
                }
#if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
#endif
            });

            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // Queries
            services.AddScoped<IComponentReadRepository, EfComponentReadRepository>();
            services.AddScoped<IComponentFamilyReadRepository, EfComponentFamilyReadRepository>();
            services.AddScoped<IFormTypeReadRepository, EfFormTypeReadRepository>();
            services.AddScoped<IParameterDefinitionReadRepository, EfParameterDefinitionReadRepository>();
            services.AddScoped<IParameterValueReadRepository, EfParameterValueReadRepository>();
            services.AddScoped<IWorkspaceQuery, EfWorkspaceQuery>();
            services.AddScoped<IComponentDetailsQuery, EfComponentDetailsQuery>();
            services.AddScoped<IFamilyDetailsQuery, EfFamilyDetailsQuery>();

            // Commands
            services.AddScoped<ISaveComponent, EfSaveComponent>();

            // Services
            services.AddSingleton<IComponentNameParser, ComponentNameParser>();
            services.AddScoped<IComponentSourceReader, AvsXmlComponentSourceReader>(); 
            
            return services;
        }



    }
}
