using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySandbox.ClassLibrary;

namespace MySandbox.Console;

public class Program
{
    public static async Task Main(string[] args)
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

        // Check for load test flag
        var isLoadTest = args.Contains("--load-test") || args.Contains("-l");
        var loadTestCount = GetLoadTestCount(args);

        if (isLoadTest)
        {
            System.Console.WriteLine($"Running load test with {loadTestCount:N0} items...\n");
            var stuffDoer = serviceProvider.GetRequiredService<ICanDoStuff>();
            await stuffDoer.ImportLargeDatasetAsync(loadTestCount);
            
            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey();
            return;
        }

        // Resolve and use services for normal operation
        var stuffDoerService = serviceProvider.GetRequiredService<ICanDoStuff>();
        var stuffDoerA = serviceProvider.GetRequiredService<ICanDoStuffA>();
        var stuffDoerB = serviceProvider.GetRequiredService<ICanDoStuffB>();

        // Execute some operations
        System.Console.WriteLine("Starting operations...\n");

        await stuffDoerA.DoStuffAAsync();
        await stuffDoerB.DoStuffBAsync();
        await stuffDoerService.DoStuffAsync();

        // Demonstrate retrieval by querying database directly (no repository loading everything)
        var dbContext = serviceProvider.GetRequiredService<StuffDbContext>();
        var totalCount = await dbContext.StuffItems.CountAsync();

        System.Console.WriteLine($"\nTotal items in database: {totalCount}");
        
        // Only show items if count is reasonable
        if (totalCount <= 100)
        {
            var items = await dbContext.StuffItems
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
                
            foreach (var item in items)
            {
                System.Console.WriteLine($"  - {item.Name} (ID: {item.Id}, Created: {item.CreatedAt:yyyy-MM-dd HH:mm:ss})");
            }
        }
        else
        {
            System.Console.WriteLine("(Too many items to display - use database queries to explore data)");
        }

        System.Console.WriteLine("\nPress any key to exit...");
        System.Console.ReadKey();
    }

    private static int GetLoadTestCount(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if ((args[i] == "--load-test" || args[i] == "-l") && int.TryParse(args[i + 1], out var count))
            {
                return count;
            }
        }
        return 1_000_000; // Default to 1 million
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
        services.Configure<SeedDataOptions>(configuration.GetSection("SeedData"));

        // Configure DbContext with SQLite
        services.AddDbContext<StuffDbContext>(options =>
        {
            options.UseSqlite("Data Source=stuff.db");
#if DEBUG
            options.EnableSensitiveDataLogging();
#endif
        });

        // Register repository (kept for backward compatibility, but direct DbContext usage is preferred)
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
