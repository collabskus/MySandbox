using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySandbox.ClassLibrary;

namespace MySandbox.Console;

public class Program
{
    public static async Task Main()
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Set up dependency injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Ensure database is created and migrated
        await EnsureDatabaseCreatedAsync(serviceProvider);

        // Resolve and use services
        var stuffDoer = serviceProvider.GetRequiredService<ICanDoStuff>();
        var stuffDoerA = serviceProvider.GetRequiredService<ICanDoStuffA>();
        var stuffDoerB = serviceProvider.GetRequiredService<ICanDoStuffB>();

        // Execute some operations
        System.Console.WriteLine("Starting operations...\n");

        await stuffDoerA.DoStuffAAsync();
        await stuffDoerB.DoStuffBAsync();
        await stuffDoer.DoStuffAsync();

        // Demonstrate retrieval by ID
        var repository = serviceProvider.GetRequiredService<IStuffRepository>();
        var allItems = await repository.GetAllAsync();

        System.Console.WriteLine($"\nTotal items in database: {allItems.Count()}");
        foreach (var item in allItems)
        {
            System.Console.WriteLine($"  - {item.Name} (ID: {item.Id}, Created: {item.CreatedAt:yyyy-MM-dd HH:mm:ss})");
        }

        System.Console.WriteLine("\nPress any key to exit...");
        System.Console.ReadKey();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Configure business rules from appsettings.json
        services.Configure<BusinessRulesOptions>(configuration.GetSection("BusinessRules"));

        // Configure DbContext with SQLite
        services.AddDbContext<StuffDbContext>(options =>
        {
            options.UseSqlite("Data Source=stuff.db");
#if DEBUG
            options.EnableSensitiveDataLogging();
#endif
        });

        // Register repository
        services.AddScoped<IStuffRepository, StuffRepository>();

        // Register StuffDoer for all three interfaces
        services.AddScoped<StuffDoer>();
        services.AddScoped<ICanDoStuff>(sp => sp.GetRequiredService<StuffDoer>());
        services.AddScoped<ICanDoStuffA>(sp => sp.GetRequiredService<StuffDoer>());
        services.AddScoped<ICanDoStuffB>(sp => sp.GetRequiredService<StuffDoer>());
    }

    private static async Task EnsureDatabaseCreatedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StuffDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database initialized successfully");
    }
}
