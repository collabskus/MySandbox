using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySandbox.ClassLibrary;
using Xunit;

namespace MySandbox.Tests;

public class IntegrationTests : DatabaseTestBase
{
    [Fact]
    public async Task FullWorkflow_AddMultipleItemsAndRetrieve()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);
        var businessRules = TestBusinessRulesOptions.Create();
        var seedData = TestSeedDataOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, DbContext, businessRules, seedData);

        // Act
        await stuffDoer.DoStuffBAsync(); // Imports Batch-Alpha, Batch-Beta, Batch-Gamma
        await repository.AddAsync("Custom Item");
        await stuffDoer.DoStuffAsync(); // Generate report

        // Assert
        var allItems = await DbContext.StuffItems.ToListAsync(TestContext.Current.CancellationToken);
        allItems.Count.Should().Be(4);
        allItems.Should().Contain(x => x.Name == "Batch-Alpha");
        allItems.Should().Contain(x => x.Name == "Batch-Beta");
        allItems.Should().Contain(x => x.Name == "Batch-Gamma");
        allItems.Should().Contain(x => x.Name == "Custom Item");
    }

    [Fact]
    public async Task DatabasePersistence_ItemsSavedAndRetrieved()
    {
        // Arrange
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);

        // Act
        await repository.AddAsync("Persistent Item");
        var itemsBeforeSave = await DbContext.StuffItems.ToListAsync(TestContext.Current.CancellationToken);
        var itemId = itemsBeforeSave.First().Id;

        var newRepository = new StuffRepository(new TestLogger<StuffRepository>(), DbContext);
        var retrievedItem = await newRepository.GetByIdAsync(itemId);

        // Assert
        retrievedItem.Should().NotBeNull();
        retrievedItem!.Name.Should().Be("Persistent Item");
    }

    [Fact]
    public async Task ConcurrentOperations_AllSucceed()
    {
        // Arrange
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);

        // Act
        var tasks = Enumerable.Range(1, 10)
            .Select(i => repository.AddAsync($"Concurrent Item {i}"))
            .ToList();

        await Task.WhenAll(tasks);

        // Assert
        var allItems = await DbContext.StuffItems.ToListAsync(TestContext.Current.CancellationToken);
        allItems.Count.Should().Be(10);
    }

    [Fact]
    public async Task CleanupWithLargeDataset_PerformsEfficiently()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create(dataRetentionDays: 30);
        var seedData = TestSeedDataOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, DbContext, businessRules, seedData);

        // Add 1000 old items
        var oldItems = Enumerable.Range(1, 1000).Select(i => new StuffItem
        {
            Name = $"Old-{i}",
            CreatedAt = DateTime.UtcNow.AddDays(-31)
        }).ToList();
        
        DbContext.StuffItems.AddRange(oldItems);
        
        // Add 100 recent items
        var recentItems = Enumerable.Range(1, 100).Select(i => new StuffItem
        {
            Name = $"Recent-{i}",
            CreatedAt = DateTime.UtcNow
        }).ToList();
        
        DbContext.StuffItems.AddRange(recentItems);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await stuffDoer.DoStuffAAsync();

        // Assert
        var remainingCount = await DbContext.StuffItems.CountAsync(TestContext.Current.CancellationToken);
        remainingCount.Should().Be(100);

        stuffDoerLogger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("Removed 1000 old items"));
    }
}
