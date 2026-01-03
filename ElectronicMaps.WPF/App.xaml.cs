using CommunityToolkit.Mvvm.Messaging;
using ElectronicMaps.Application;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.Infrastructure;
using ElectronicMaps.Infrastructure.Initialization;
using ElectronicMaps.Infrastructure.Services;
using ElectronicMaps.WPF.Features.Welcome;
using ElectronicMaps.WPF.Features.Workspace;
using ElectronicMaps.WPF.Infrastructure.Commands;
using ElectronicMaps.WPF.Infrastructure.Commands.XmlCommands;
using ElectronicMaps.WPF.Main;
using ElectronicMaps.WPF.Services.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Navigation.Core.Abstractions;
using Navigation.Core.Services;
using System.Text;
using System.Windows;

namespace ElectronicMaps.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private IHost _host = null!;


       


        protected override void OnStartup(StartupEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.local.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
                }) 
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.AddSingleton<IMessenger, WeakReferenceMessenger>();

                    services.AddMapsInfrastructure(configuration);
                    services.AddApplication(configuration);

                    services.AddSingleton<INavigationService, NavigationService>();

                    // ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<WelcomeViewModel>();
                    services.AddTransient<WorkspaceViewModel>();

                    services.AddScoped<IComponentNameParser, ComponentNameParser>();
                    services.AddSingleton<IXmlCommands, XmlCommands>();
                    services.AddSingleton<IAppCommands, AppCommands>();

                    services.AddSingleton<IDialogService, DialogService>();
                })
                .Build();

            // Инициализация БД
            InitializeDatabaseAsync().GetAwaiter().GetResult();

            // Запуск главного окна
            ShowMainWindowAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeDatabaseAsync()
        {
            using var scope = _host.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
            await initializer.InitializeAsync();
        }

        private async Task ShowMainWindowAsync()
        {
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            await mainViewModel.InitializeAsync();

            var mainWindow = new MainWindow();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.Dispose();
            base.OnExit(e);
        }


    }

}
