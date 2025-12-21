using ElectronicMaps.Application.Project;
using ElectronicMaps.Application.Security;
using ElectronicMaps.Application.Services;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Application.WorkspaceProject;
using ElectronicMaps.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IFileImportService, FileImportService>();
            services.AddScoped<IComponentAnalysisService, ComponentAnalysisService>();


            services.AddSingleton<IComponentStore, ComponentStore>();
            services.AddSingleton<IComponentStoreSerializer, JsonComponentStoreSerializer>();
            services.AddSingleton<IProjectSaveService, ProjectSaveService>(); // TODO: Move to Infrastructure>
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IWorkspaceProjectSerializer, ZipWorkspaceProjectSerializer>(); // TODO: Move to Infrastructure>

            return services;
        }





    }
}
