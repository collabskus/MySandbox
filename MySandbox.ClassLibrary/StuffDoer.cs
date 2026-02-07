using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MySandbox.ClassLibrary;

public class StuffDoer(
    ILogger<StuffDoer> logger,
    IStuffRepository repository,
    IOptions<BusinessRulesOptions> businessRules,
    IOptions<SeedDataOptions> seedData)
    : ICanDoStuff, ICanDoStuffA, ICanDoStuffB
{
    private readonly ILogger<StuffDoer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IStuffRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly BusinessRulesOptions _businessRules = businessRules?.Value ?? throw new ArgumentNullException(nameof(businessRules));
    private readonly SeedDataOptions _seedData = seedData?.Value ?? throw new ArgumentNullException(nameof(seedData));

    public async Task DoStuffAsync()
    {
        _logger.LogInformation("Generating daily report");
        var items = await _repository.GetAllAsync();

        var today = DateTime.UtcNow.Date;
        var todaysItems = items.Where(x => x.CreatedAt.Date == today).ToList();

        _logger.LogInformation(
            "Daily Report: {TodayCount} items created today, {TotalCount} items total",
            todaysItems.Count,
            items.Count());
    }

    public async Task DoStuffAAsync()
    {
        _logger.LogInformation("Running cleanup process for items older than {Days} days",
            _businessRules.DataRetentionDays);

        var items = await _repository.GetAllAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-_businessRules.DataRetentionDays);
        var oldItems = items.Where(x => x.CreatedAt < cutoffDate).ToList();

        foreach (var item in oldItems)
        {
            await _repository.RemoveAsync(item.Id);
        }

        _logger.LogInformation("Cleanup completed: Removed {Count} old items", oldItems.Count);
    }

    public async Task DoStuffBAsync()
    {
        _logger.LogInformation("Importing sample data with prefix '{Prefix}'",
            _businessRules.BatchOperationPrefix);

        var sampleData = _seedData.Products;

        var existingItems = await _repository.GetAllAsync();
        var existingNames = existingItems.Select(x => x.Name).ToHashSet();

        var newItemsAdded = 0;

        foreach (var item in sampleData)
        {
            var fullName = $"{_businessRules.BatchOperationPrefix}{item}";

            if (!existingNames.Contains(fullName))
            {
                await _repository.AddAsync(fullName);
                newItemsAdded++;
            }
        }

        _logger.LogInformation(
            "Import completed: {NewCount} new items added, {SkippedCount} duplicates skipped",
            newItemsAdded,
            sampleData.Count - newItemsAdded);
    }
}
