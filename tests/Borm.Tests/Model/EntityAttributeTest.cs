using Borm.Model;

namespace Borm.Tests.Model;

public sealed class EntityAttributeTest
{
    [Fact]
    public void Constructor_InitializesNewInstance()
    {
        // Act
        EntityAttribute attribute = new();

        // Assert
        Assert.Null(attribute.Name);
    }

    [Fact]
    public void Constructor_InitializesNewInstance_WithName()
    {
        // Arrange
        string name = "foo";

        // Act
        EntityAttribute attribute = new(name);

        // Assert
        Assert.Equal(name, attribute.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_ThrowsArgumentException_WithInvalidName(string name)
    {
        // Act
        Exception? exception = Record.Exception(() => _ = new EntityAttribute(name));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullName()
    {
        // Act
        Exception? exception = Record.Exception(() => _ = new EntityAttribute(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }
}
