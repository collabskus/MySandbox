using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using MySandbox.ClassLibrary;
using Xunit;

namespace MySandbox.Tests;

public class StuffDbContextTests : DatabaseTestBase
{
    [Fact]
    public async Task DbContext_StuffItems_IsAccessible()
    {
        // Act
        var stuffItems = DbContext.StuffItems;

        // Assert
        stuffItems.Should().NotBeNull();
        var count = await stuffItems.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(0);
    }

    [Fact]
    public async Task DbContext_EntityConfiguration_EnforcesConstraints()
    {
        // Arrange
        var item = new StuffItem
        {
            Name = "Test",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        DbContext.StuffItems.Add(item);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedItem = await DbContext.StuffItems.FirstAsync(TestContext.Current.CancellationToken);
        savedItem.Id.Should().BeGreaterThan(0);
        savedItem.Name.Should().Be("Test");
    }

    [Fact]
    public async Task DbContext_RequiredName_EnforcedByDatabase()
    {
        // Arrange
        var item = new StuffItem
        {
            Name = null!,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        DbContext.StuffItems.Add(item);
        await Assert.ThrowsAsync<DbUpdateException>(async () => await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}
