using AwesomeAssertions;
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
        var stuffDoer = new StuffDoer(stuffDoerLogger, repository, businessRules, seedData);

        // Act
        await stuffDoer.DoStuffBAsync(); // Imports Batch-Alpha, Batch-Beta, Batch-Gamma
        await repository.AddAsync("Custom Item");
        await stuffDoer.DoStuffAsync(); // Generate report

        // Assert
        var allItems = await repository.GetAllAsync();
        allItems.Count().Should().Be(4);
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
        var itemsBeforeSave = await repository.GetAllAsync();
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
        var allItems = await repository.GetAllAsync();
        allItems.Count().Should().Be(10);
    }
}
