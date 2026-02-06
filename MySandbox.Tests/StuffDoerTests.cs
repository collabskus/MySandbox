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
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);
        var businessRules = TestBusinessRulesOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, repository, businessRules);

        await repository.AddAsync("Today Item 1");
        await repository.AddAsync("Today Item 2");

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
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);
        var businessRules = TestBusinessRulesOptions.Create(dataRetentionDays: 30);
        var stuffDoer = new StuffDoer(stuffDoerLogger, repository, businessRules);

        // Add old item (manually set CreatedAt to simulate old data)
        var oldItem = new StuffItem
        {
            Name = "Old Item",
            CreatedAt = DateTime.UtcNow.AddDays(-31)
        };
        DbContext.StuffItems.Add(oldItem);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Add recent item
        await repository.AddAsync("Recent Item");

        // Act
        await stuffDoer.DoStuffAAsync();

        // Assert
        var remainingItems = await repository.GetAllAsync();
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
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);
        var businessRules = TestBusinessRulesOptions.Create(batchOperationPrefix: "Test-");
        var stuffDoer = new StuffDoer(stuffDoerLogger, repository, businessRules);

        // Act
        await stuffDoer.DoStuffBAsync();

        // Assert
        var items = await repository.GetAllAsync();
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
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);
        var businessRules = TestBusinessRulesOptions.Create();
        var stuffDoer = new StuffDoer(stuffDoerLogger, repository, businessRules);

        // Pre-add one item
        await repository.AddAsync("Batch-Alpha");

        // Act
        await stuffDoer.DoStuffBAsync();

        // Assert
        var items = await repository.GetAllAsync();
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
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);
        var businessRules = TestBusinessRulesOptions.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StuffDoer(null!, repository, businessRules));
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var businessRules = TestBusinessRulesOptions.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StuffDoer(stuffDoerLogger, null!, businessRules));
    }

    [Fact]
    public void Constructor_NullBusinessRules_ThrowsArgumentNullException()
    {
        // Arrange
        var stuffDoerLogger = new TestLogger<StuffDoer>();
        var repositoryLogger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(repositoryLogger, DbContext);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StuffDoer(stuffDoerLogger, repository, null!));
    }
}
