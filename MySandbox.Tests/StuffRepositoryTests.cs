using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySandbox.ClassLibrary;
using Xunit;

namespace MySandbox.Tests;

public class StuffRepositoryTests : DatabaseTestBase
{
    [Fact]
    public async Task AddAsync_ValidItem_AddsToDatabase()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);
        var itemName = "Test Item";

        // Act 
        await repository.AddAsync(itemName);

        // Assert
        var items = await DbContext.StuffItems.ToListAsync(TestContext.Current.CancellationToken);

        // Chain the count check, the name check, and the date check together
        items.Should().ContainSingle()
            .Which.Name.Should().Be(itemName);

        // Use .And.Subject to stay on the same item for the date check
        items.First().CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Clean logger assertion
        logger.LogEntries.Should().ContainSingle(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains($"Added item: {itemName}"));
    }

    [Fact]
    public async Task AddAsync_NullItem_ThrowsArgumentException()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await repository.AddAsync(null!));

        logger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Warning &&
            x.Message.Contains("null or empty"));
    }

    [Fact]
    public async Task AddAsync_EmptyItem_ThrowsArgumentException()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await repository.AddAsync("   "));
    }

    [Fact]
    public async Task AddAsync_ItemExceedingMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);
        var longItem = new string('A', 501);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await repository.AddAsync(longItem));

        logger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Warning &&
            x.Message.Contains("exceeding maximum length"));
    }

    [Fact]
    public async Task AddAsync_MaxLengthItem_Succeeds()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);
        var maxLengthItem = new string('A', 500);

        // Act
        await repository.AddAsync(maxLengthItem);

        // Assert
        var items = await DbContext.StuffItems.ToListAsync(TestContext.Current.CancellationToken);
        items.Should().ContainSingle()
            .Which.Name.Should().Be(maxLengthItem);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyCollection()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
        logger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("Getting all items"));
    }

    [Fact]
    public async Task GetAllAsync_WithItems_ReturnsAllItemsOrderedByCreatedAt()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);

        await repository.AddAsync("Item 1");
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await repository.AddAsync("Item 2");
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await repository.AddAsync("Item 3");

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        var items = result.ToList();
        items.Count.Should().Be(3);
        items[0].Name.Should().Be("Item 1");
        items[1].Name.Should().Be("Item 2");
        items[2].Name.Should().Be("Item 3");
        items[0].CreatedAt.Should().BeBefore(items[1].CreatedAt);
        items[1].CreatedAt.Should().BeBefore(items[2].CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsItem()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);
        await repository.AddAsync("Test Item");
        var allItems = await repository.GetAllAsync();
        var itemId = allItems.First().Id;

        // Act
        var result = await repository.GetByIdAsync(itemId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Item");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
        logger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Warning &&
            x.Message.Contains("not found"));
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);

        // Act
        var result = await repository.GetByIdAsync(-1);

        // Assert
        result.Should().BeNull();
        logger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Warning &&
            x.Message.Contains("Invalid ID"));
    }

    [Fact]
    public async Task RemoveAsync_ExistingId_RemovesItemAndReturnsTrue()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);
        await repository.AddAsync("Item to Remove");
        var allItems = await repository.GetAllAsync();
        var itemId = allItems.First().Id;

        // Act
        var result = await repository.RemoveAsync(itemId);

        // Assert
        result.Should().BeTrue();
        var remainingItems = await repository.GetAllAsync();
        remainingItems.Should().BeEmpty();
        logger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Information &&
            x.Message.Contains("Removed item"));
    }

    [Fact]
    public async Task RemoveAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);

        // Act
        var result = await repository.RemoveAsync(999);

        // Assert
        result.Should().BeFalse();
        logger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Warning &&
            x.Message.Contains("Cannot remove"));
    }

    [Fact]
    public async Task RemoveAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        var logger = new TestLogger<StuffRepository>();
        var repository = new StuffRepository(logger, DbContext);

        // Act
        var result = await repository.RemoveAsync(0);

        // Assert
        result.Should().BeFalse();
        logger.LogEntries.Should().Contain(x =>
            x.LogLevel == LogLevel.Warning &&
            x.Message.Contains("Invalid ID for removal"));
    }
}
