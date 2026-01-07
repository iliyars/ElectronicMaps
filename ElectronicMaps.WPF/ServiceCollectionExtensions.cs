using ElectronicMaps.WPF.Features.Welcome;
using ElectronicMaps.WPF.Features.Workspace;
using ElectronicMaps.WPF.Features.Workspace.Components.ViewModels;
using ElectronicMaps.WPF.Infrastructure.Commands;
using ElectronicMaps.WPF.Main;
using ElectronicMaps.WPF.Services.Dialogs;
using ElectronicMaps.WPF.Services.Factories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ElectronicMaps.WPF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            // Services
            services.AddSingleton<IDialogService, DialogService>();

            // Factories 
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
            services.AddSingleton<ICardViewModelFactory, CardViewModelFactory>();

            // Commands
            services.AddSingleton<IApplicationCommands, ApplicationCommands>();


            //ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<WelcomeViewModel>();
            services.AddTransient<WorkspaceViewModel>();
            services.AddTransient<CreateComponentViewModel>();

            return services; 
        }

    }
}
