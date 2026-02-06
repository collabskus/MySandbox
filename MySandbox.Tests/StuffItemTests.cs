using MySandbox.ClassLibrary;
using Xunit;
using AwesomeAssertions;

namespace MySandbox.Tests;

public class StuffItemTests
{
    [Fact]
    public void StuffItem_DefaultValues_AreCorrect()
    {
        // Act
        var item = new StuffItem();

        // Assert
        item.Should().BeEquivalentTo(new
        {
            Id = 0,
            Name = string.Empty,
            CreatedAt = default(DateTime)
        });
    }

    [Fact]
    public void StuffItem_SetProperties_WorksCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var item = new StuffItem
        {
            Id = 42,
            Name = "Test",
            CreatedAt = now
        };

        // Assert
        item.Should().BeEquivalentTo(new
        {
            Id = 42,
            Name = "Test",
            CreatedAt = now
        });
    }
}
