using CommunityToolkit.Mvvm.Messaging;
using ElectronicMaps.Application;
using ElectronicMaps.Infrastructure;
using ElectronicMaps.Infrastructure.Persistence.Initialization;
using ElectronicMaps.WPF.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Navigation.Core.Abstractions;
using Navigation.Core.Services;
using Serilog;
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

        protected override async void OnStartup(StartupEventArgs e)
        {
            ConfigureSerialog();

            // Логируем старт приложения
            Log.Information("========== Application Starting ==========");
            Log.Information("Version: {Version}", GetType().Assembly.GetName().Version);

            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                base.OnStartup(e);

                _host = Host.CreateDefaultBuilder()
                    .UseSerilog()
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

                        // Common Services
                        services.AddSingleton<IMessenger, WeakReferenceMessenger>();
                        services.AddSingleton<INavigationService, NavigationService>();

                        // Layers registration
                        services.AddMapsInfrastructure(configuration);
                        services.AddApplication(configuration);
                        services.AddPresentation();
                    })
                    .Build();

                Log.Information("Host created successfully");

                // Инициализация БД
                Log.Information("Initializing database...");
                await InitializeDatabaseAsync();
                Log.Information("Database initialized successfully");

                // Запуск главного окна
                Log.Information("Showing main window...");
                await ShowMainWindowAsync();
                Log.Information("Main window shown successfully");
            }catch(Exception ex)
            {
                // Критическая ошибка при старте
                Log.Fatal(ex, "Application failed to start");

                MessageBox.Show(
                    $"Не удалось запустить приложение:\n\n{ex.Message}\n\nПодробности в logs/log-{DateTime.Now:yyyyMMdd}.txt",
                    "Ошибка запуска",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(1);
            }
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

        private void ConfigureSerialog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Mocrosoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)

                // Обогащение логов
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "ElectronicMaps")

#if DEBUG
                .WriteTo.Debug(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")

                .WriteTo.File(
                    path: "logs/debug-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
#else

                // В Release - только важные логи
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,  // Хранить 30 дней
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
#endif
                // Отдельный файл для ошибок (всегда)
                .WriteTo.File(
                    path: "logs/errors-.log",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                    retainedFileCountLimit: 90,  // Хранить 90 дней для ошибок
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

    }

}
