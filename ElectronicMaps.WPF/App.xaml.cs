using CommunityToolkit.Mvvm.Messaging;
using ElectronicMaps.Application;
using ElectronicMaps.Application.Services;
using ElectronicMaps.Domain.Services;
using ElectronicMaps.Infrastructure;
using ElectronicMaps.Infrastructure.Services;
using ElectronicMaps.WPF.Features.Welcome;
using ElectronicMaps.WPF.Infrastructure.Commands;
using ElectronicMaps.WPF.Infrastructure.Commands.XmlCommands;
using ElectronicMaps.WPF.Infrastructure.Navigation;
using ElectronicMaps.WPF.Infrastructure.ViewLocation;
using ElectronicMaps.WPF.Services.Dialogs;
using ElectronicMaps.WPF.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
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
                    .AddEnvironmentVariables();
                }) 
                .ConfigureServices((context, services) =>
                {

                    var configuration = context.Configuration;

                    services.AddSingleton<IMessenger, WeakReferenceMessenger>(); // ← ДОБАВЬТЕ ЭТУ СТРОКУ!


                    services.AddMapsInfrastructure(configuration);
                    services.AddApplication();

                    // 1. Регистрируем IScreenRegistry как Singleton
                    services.AddSingleton<IScreenRegistry>(provider =>
                    {
                        var registry = new ScreenRegistry();

                        // 2. Автоматически регистрируем все экраны по атрибуту [Screen]
                        //    в нужных сборках (как минимум — сборка WPF-приложения)
                        ScreenRegistrar.RegisterScreensFromAssemblies(
                            registry,
                            typeof(WelcomeViewModel).Assembly // тут твоя WPF-assembly с VM
                                                              // если будут отдельные сборки с экранами — добавишь их сюда
                        );

                        return registry;
                    });
                    services.AddSingleton<INavigationService, NavigationService>();


                    services.AddTransient<MainViewModel>();
                    services.AddTransient<WelcomeViewModel>();
                    services.AddScoped<IComponentNameParser, ComponentNameParser>();
                    services.AddScoped<IFileImportService, FileImportService>();
                    services.AddScoped<IComponentFormBatchService, ComponentFormBatchService>();
                    services.AddScoped<IFormQueryService, FormQueryService>();
                    services.AddSingleton<IXmlCommands, XmlCommands>();
                    services.AddSingleton<IAppCommands, AppCommands>();

                    services.AddSingleton<IDialogService, DialogService>();
                })
                .Build();

            using (var scope = _host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
            ViewTemplateRegistrar.RegisterViewTemplates(typeof(WelcomeViewModel).Assembly);
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            var mainWindow = new MainWindow();
            mainWindow.DataContext = mainViewModel;
            mainViewModel.InitializeAsync().GetAwaiter().GetResult();

            mainWindow.Show();

        }
    }

}
