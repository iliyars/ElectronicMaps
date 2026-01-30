using ElectronicMaps.Application.Abstractions.Services;
using ElectronicMaps.Application.Features.Components.Services;
using ElectronicMaps.Application.Features.Import.Services;
using ElectronicMaps.Application.Features.Workspace.Serialization;
using ElectronicMaps.Application.Features.Workspace.Services;
using ElectronicMaps.Application.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ElectronicMaps.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            // Mediator
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            });

            // Services
            services.AddScoped<IComponentFormBatchService, ComponentFormBatchService>();
            services.AddScoped<IFileImportService, FileImportService>();
            services.AddScoped<IComponentAnalysisService, ComponentAnalysisService>();
            services.AddScoped<IComponentCreationService, ComponentCreationService>();
            services.AddScoped<IComponentQueryService, ComponentQueryService>();
            services.AddScoped<IComponentFamilyQueryService, ComponentFamilyQueryService>();

            // Commands

            // Stores
            services.AddSingleton<IComponentStore, ComponentStore>();

            //Serialization
            services.AddSingleton<IComponentStoreSerializer, JsonComponentStoreSerializer>();
            services.AddScoped<IWorkspaceProjectSerializer, ZipWorkspaceProjectSerializer>();

            // Project Management
            services.AddSingleton<IProjectSaveService, ProjectSaveService>();



            return services;
        }
    }
}
