using ElectronicMaps.Documents.Configuration;
using ElectronicMaps.Documents.Services;
using ElectronicMaps.Documents.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Documents.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddWordDocumentGenerator(
            this IServiceCollection services,
            Action<DocumentGeneratorOptions>? configureOptions = null)
        {
            // Регистрируем опции
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                // Дефолтные опции
                services.Configure<DocumentGeneratorOptions>(options => { });
            }

            services.AddSingleton<ITemplateSchemaStore, JsonTemplateSchemaStore>();
            services.AddScoped<IDocumentGenerator, WordDocumentGenerator>();

            return services;
        }

        public static IServiceCollection AddWordDocumentGenerator(
            this IServiceCollection services,
            string templatesDirectory,
            string schemasDirectory,
            string? outputDirectory = null)
        {
            services.Configure<DocumentGeneratorOptions>(options =>
            {
                options.TemplatesDirectory = templatesDirectory;
                options.SchemasDirectory = schemasDirectory;

                if (!string.IsNullOrEmpty(outputDirectory))
                {
                    options.OutputDirectory = outputDirectory;
                }
            });

            // Регистрируем сервисы
            services.AddSingleton<ITemplateSchemaStore, JsonTemplateSchemaStore>();
            services.AddScoped<IDocumentGenerator, WordDocumentGenerator>();

            return services;
        }

        public static IServiceCollection AddWordDocumentGenerator(
            this IServiceCollection services,
            Action<DocumentGeneratorOptions> configureOptions,
            bool validateOnStartup)
        {
            services.Configure(configureOptions);

            if (validateOnStartup)
            {
                // PostConfigure вызывается после всех Configure
                services.PostConfigure<DocumentGeneratorOptions>(options =>
                {
                    // Валидация настроек при старте приложения
                    options.Validate();
                });
            }

            services.AddSingleton<ITemplateSchemaStore, JsonTemplateSchemaStore>();
            services.AddScoped<IDocumentGenerator, WordDocumentGenerator>();

            return services;
        }
    }
}
