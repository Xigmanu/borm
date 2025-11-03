using Borm.Model;

namespace Borm.Tests.Model;

public sealed class ColumnAttributeTest
{
    [Fact]
    public void Constructor_InitializesNewInstance_WithIndexAndName()
    {
        // Arrange
        string name = "foo";
        int index = 0;

        // Act
        ColumnAttribute attribute = new(index, name);

        // Assert
        Assert.Equal(name, attribute.Name);
        Assert.Equal(index, attribute.Index);
    }

    [Fact]
    public void Constructor_InitializesNewInstance_WithIndexAndNullName()
    {
        // Arrange
        int index = 0;

        // Act
        ColumnAttribute attribute = new(index);

        // Assert
        Assert.Null(attribute.Name);
        Assert.Equal(index, attribute.Index);
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WithInvalidIndex()
    {
        // Arrange
        int index = -1;

        // Act
        Exception? exception = Record.Exception(() => _ = new ColumnAttribute(index));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_ThrowsArgumentException_WithInvalidName(string name)
    {
        // Arrange
        int index = 0;

        // Act
        Exception? exception = Record.Exception(() => _ = new ColumnAttribute(index, name));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullName()
    {
        // Arrange
        int index = 0;

        // Act
        Exception? exception = Record.Exception(() => _ = new ColumnAttribute(index, null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }
}
