using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MySandbox.ClassLibrary;

public class StuffDoer(
    ILogger<StuffDoer> logger,
    StuffDbContext dbContext,
    IOptions<BusinessRulesOptions> businessRules,
    IOptions<SeedDataOptions> seedData)
    : ICanDoStuff, ICanDoStuffA, ICanDoStuffB
{
    private readonly ILogger<StuffDoer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly StuffDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly BusinessRulesOptions _businessRules = businessRules?.Value ?? throw new ArgumentNullException(nameof(businessRules));
    private readonly SeedDataOptions _seedData = seedData?.Value ?? throw new ArgumentNullException(nameof(seedData));

    public async Task DoStuffAsync()
    {
        _logger.LogInformation("Generating daily report");
        
        // FIXED: Use database-side filtering instead of loading everything into memory
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        
        var todayCount = await _dbContext.StuffItems
            .Where(x => x.CreatedAt >= today && x.CreatedAt < tomorrow)
            .CountAsync();
        
        var totalCount = await _dbContext.StuffItems.CountAsync();

        _logger.LogInformation(
            "Daily Report: {TodayCount} items created today, {TotalCount} items total",
            todayCount,
            totalCount);
    }

    public async Task DoStuffAAsync()
    {
        _logger.LogInformation("Running cleanup process for items older than {Days} days",
            _businessRules.DataRetentionDays);

        // FIXED: Use ExecuteDeleteAsync for bulk deletion instead of loading into memory and deleting one-by-one
        var cutoffDate = DateTime.UtcNow.AddDays(-_businessRules.DataRetentionDays);
        
        var deletedCount = await _dbContext.StuffItems
            .Where(x => x.CreatedAt < cutoffDate)
            .ExecuteDeleteAsync();

        _logger.LogInformation("Cleanup completed: Removed {Count} old items", deletedCount);
    }

    public async Task DoStuffBAsync()
    {
        _logger.LogInformation("Importing sample data with prefix '{Prefix}'",
            _businessRules.BatchOperationPrefix);

        var sampleData = _seedData.Products;

        // FIXED: Use database-side duplicate checking instead of loading all existing items
        var newItemsAdded = 0;
        var skippedCount = 0;

        foreach (var item in sampleData)
        {
            var fullName = $"{_businessRules.BatchOperationPrefix}{item}";

            // Check if item exists using a database query instead of in-memory collection
            var exists = await _dbContext.StuffItems
                .AnyAsync(x => x.Name == fullName);

            if (!exists)
            {
                _dbContext.StuffItems.Add(new StuffItem
                {
                    Name = fullName,
                    CreatedAt = DateTime.UtcNow
                });
                newItemsAdded++;
            }
            else
            {
                skippedCount++;
            }
        }

        if (newItemsAdded > 0)
        {
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Import completed: {NewCount} new items added, {SkippedCount} duplicates skipped",
            newItemsAdded,
            skippedCount);
    }

    public async Task ImportLargeDatasetAsync(int count = 1_000_000)
    {
        _logger.LogInformation("Importing {Count} items for performance testing", count);

        const int batchSize = 10000;
        var itemsAdded = 0;

        for (var i = 0; i < count; i += batchSize)
        {
            var currentBatchSize = Math.Min(batchSize, count - i);
            var items = new List<StuffItem>(currentBatchSize);

            for (var j = 0; j < currentBatchSize; j++)
            {
                items.Add(new StuffItem
                {
                    Name = $"LoadTest-{i + j + 1}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbContext.StuffItems.AddRangeAsync(items);
            await _dbContext.SaveChangesAsync();
            
            itemsAdded += currentBatchSize;
            
            if (itemsAdded % 50000 == 0)
            {
                _logger.LogInformation("Progress: {ItemsAdded}/{TotalCount} items added", itemsAdded, count);
            }
        }

        _logger.LogInformation("Large dataset import completed: {Count} items added", itemsAdded);
    }
}
