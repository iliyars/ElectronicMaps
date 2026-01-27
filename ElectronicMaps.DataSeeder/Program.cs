
using DocumentFormat.OpenXml.Wordprocessing;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using ElectronicMaps.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var services = new ServiceCollection();

var csvFileFormTypes = @"D:\dev\csharp\ElectronicMaps\ElectronicMaps.DataSeeder\Data\FormTypes.csv";
var csvFileParameterDefinitions = @"D:\dev\csharp\ElectronicMaps\ElectronicMaps.DataSeeder\Data\ParameterDefinitions.csv";

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

Console.WriteLine("========================================");
Console.WriteLine("  ElectronicMaps Database Seeder");
Console.WriteLine("========================================\n");
// Logging
services.AddLogging(builder => builder.AddConsole());

// DbContext
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=D:\\dev\\databases\\electronicmaps.db"));
//Importer
services.AddScoped<DatabaseCsvImport>();

var provider = services.BuildServiceProvider();

// Импорт
using var scope = provider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var importer = scope.ServiceProvider.GetRequiredService<DatabaseCsvImport>();

// Создать БД
await context.Database.EnsureCreatedAsync();

// FormTypes
var formTypesResult = await importer.ImportFormTypesAsync(csvFileFormTypes);
formTypesResult.PrintSummary();

await importer.ClearTableAsync("ParameterDefinitions");

// ParameterDefinition
var parametersResult = await importer.ImportParametersDefinitionsAsync(csvFileParameterDefinitions, 1);
parametersResult.PrintSummary();

Console.WriteLine("Done! Press any key...");
Console.ReadKey();