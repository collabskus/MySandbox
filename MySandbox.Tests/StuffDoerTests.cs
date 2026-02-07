using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using MySandbox.ClassLibrary;
using Xunit;

namespace MySandbox.Tests;

public class StuffDoerTests : DatabaseTestBase
{
    [Fact]
    public async Task DoStuffAsync_GeneratesDailyReport()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create();
        var seedData = TestSeedDataOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, DbContext, businessRules, seedData);

        DbContext.StuffItems.Add(new StuffItem { Name = "Today Item 1", CreatedAt = DateTime.UtcNow });
        DbContext.StuffItems.Add(new StuffItem { Name = "Today Item 2", CreatedAt = DateTime.UtcNow });
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await stuffDoer.DoStuffAsync();

        // Assert
        stuffDoerLogger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("Generating daily report"));

        stuffDoerLogger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("2 items created today"));
    }

    [Fact]
    public async Task DoStuffAAsync_CleansUpOldItems()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create(dataRetentionDays: 30);
        var seedData = TestSeedDataOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, DbContext, businessRules, seedData);

        // Add old item
        var oldItem = new StuffItem
        {
            Name = "Old Item",
            CreatedAt = DateTime.UtcNow.AddDays(-31)
        };
        DbContext.StuffItems.Add(oldItem);
        
        // Add recent item
        var recentItem = new StuffItem
        {
            Name = "Recent Item",
            CreatedAt = DateTime.UtcNow
        };
        DbContext.StuffItems.Add(recentItem);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await stuffDoer.DoStuffAAsync();

        // Assert
        var remainingItems = await DbContext.StuffItems.ToListAsync(TestContext.Current.CancellationToken);
        remainingItems.Should().ContainSingle().Which.Name.Should().Be("Recent Item");

        stuffDoerLogger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("Removed 1 old items"));
    }

    [Fact]
    public async Task DoStuffBAsync_ImportsDataWithPrefix()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create(batchOperationPrefix: "Test-");
        var seedData = TestSeedDataOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, DbContext, businessRules, seedData);

        // Act
        await stuffDoer.DoStuffBAsync();

        // Assert
        var items = await DbContext.StuffItems.ToListAsync(TestContext.Current.CancellationToken);
        items.Select(x => x.Name).Should().BeEquivalentTo("Test-Alpha", "Test-Beta", "Test-Gamma");

        stuffDoerLogger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("3 new items added"));
    }

    [Fact]
    public async Task DoStuffBAsync_SkipsDuplicates()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create();
        var seedData = TestSeedDataOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, DbContext, businessRules, seedData);

        // Pre-add one item
        DbContext.StuffItems.Add(new StuffItem { Name = "Batch-Alpha", CreatedAt = DateTime.UtcNow });
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await stuffDoer.DoStuffBAsync();

        // Assert
        var items = await DbContext.StuffItems.ToListAsync(TestContext.Current.CancellationToken);
        items.Should().HaveCount(3)
            .And.Subject.Select(x => x.Name)
            .Should().Contain(["Batch-Beta", "Batch-Gamma"]);

        stuffDoerLogger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("2 new items added") &&
            x.Message.Contains("1 duplicates skipped"));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var businessRules = TestBusinessRulesOptions.Create();
        var seedData = TestSeedDataOptions.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StuffDoer(null!, DbContext, businessRules, seedData));
    }

    [Fact]
    public void Constructor_NullDbContext_ThrowsArgumentNullException()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create();
        var seedData = TestSeedDataOptions.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StuffDoer(stuffDoerLogger, null!, businessRules, seedData));
    }

    [Fact]
    public void Constructor_NullBusinessRules_ThrowsArgumentNullException()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var seedData = TestSeedDataOptions.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StuffDoer(stuffDoerLogger, DbContext, null!, seedData));
    }

    [Fact]
    public void Constructor_NullSeedData_ThrowsArgumentNullException()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StuffDoer(stuffDoerLogger, DbContext, businessRules, null!));
    }

    [Fact]
    public async Task ImportLargeDatasetAsync_CreatesSpecifiedNumberOfItems()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create();
        var seedData = TestSeedDataOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, DbContext, businessRules, seedData);

        // Act - Import a small number for testing
        await stuffDoer.ImportLargeDatasetAsync(100);

        // Assert
        var count = await DbContext.StuffItems.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(100);

        stuffDoerLogger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("Large dataset import completed"));
    }
}
