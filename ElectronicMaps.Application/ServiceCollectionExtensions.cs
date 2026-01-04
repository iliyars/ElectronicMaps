using ElectronicMaps.Application.Features.Components.Services;
using ElectronicMaps.Application.Features.Import.Services;
using ElectronicMaps.Application.Features.Workspace.Serialization;
using ElectronicMaps.Application.Features.Workspace.Services;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Documents.DI;
using ElectronicMaps.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IFileImportService, FileImportService>();
            services.AddScoped<IComponentAnalysisService, ComponentAnalysisService>();
            services.AddSingleton<IComponentStore, ComponentStore>();
            services.AddSingleton<IComponentStoreSerializer, JsonComponentStoreSerializer>();
            services.AddSingleton<IProjectSaveService, ProjectSaveService>(); // TODO: Move to Infrastructure>
            services.AddScoped<IWorkspaceProjectSerializer, ZipWorkspaceProjectSerializer>(); // TODO: Move to Infrastructure>

            //services.AddScoped<IDocumentAdapter, DocumentAdapter>(); // TODO: Move to Infrastructure>
            //services.AddScoped<IDocumentGenerationService, DocumentGenerationService>(); // TODO: Move to Infrastructure>

            services.AddDocumentRendering(configuration);
            return services;
        }
    }
}
