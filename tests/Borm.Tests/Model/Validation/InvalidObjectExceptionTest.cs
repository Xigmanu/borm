using Borm.Model.Validation;
using Borm.Properties;

namespace Borm.Tests.Model.Validation;

public sealed class InvalidObjectExceptionTest
{
    [Fact]
    public void Constructor_InitializesNewInstanceWithValidMessage_WithValidResult()
    {
        // Arrange
        string message = "message";
        string column = "name";
        string entity = "entity";

        string expected = $"{Strings.ValidationFailedWithMetadata(column, entity)} {message}";

        ValidationResult result = new(message, entity, column, true);

        // Act
        InvalidObjectException exception = new(result);

        // Assert
        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WithInvalidResult()
    {
        // Arrange
        string message = "message";
        string column = "name";
        string entity = "entity";

        ValidationResult result = new(message, entity, column, false);

        // Act
        Exception? exception = Record.Exception(() => _ = new InvalidObjectException(result));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }
}