using ElectronicMaps.Documents.Configuration;
using ElectronicMaps.Documents.Core.Services;
using ElectronicMaps.Documents.Rendering.FormRenderer;
using ElectronicMaps.Documents.Rendering.OpenXml;
using ElectronicMaps.Documents.Rendering.Schemas.Store;
using ElectronicMaps.Documents.Rendering.Validation;
using ElectronicMaps.Documents.Templates.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.DI
{
    public static class ServiceCollectionExtentions
    {

        public static IServiceCollection AddDocumentRendering(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<DocumentRenderingOptions>(configuration.GetSection("DocumentRendering"));


            services.AddSingleton<ISchemaStore, JsonSchemaStore>(sp =>
            {
                var optons = sp.GetRequiredService<IOptions<DocumentRenderingOptions>>().Value;
                return new JsonSchemaStore(optons.SchemasPath);
            });

            services.AddSingleton<ITemplateStore, FileSystemTemplateStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<DocumentRenderingOptions>>().Value;
                return new FileSystemTemplateStore(options.TemplatesPath);
            });

            // OpenXML компоненты
            services.AddSingleton<SdtContentWriter>();
            services.AddSingleton<TableCloner>();

            // Form Renderer
            services.AddSingleton<GenericFormRenderer>();

            // Главный рендерер
            services.AddScoped<IWordRenderer, OpenXmlWordRenderer>();

            // Валидация (опционально)
            services.AddSingleton<ITemplateValidator, TemplateValidator>();

            return services;

        }


    }
}
