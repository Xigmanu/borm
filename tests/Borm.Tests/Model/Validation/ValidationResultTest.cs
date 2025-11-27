using Borm.Model.Validation;
using Borm.Properties;

namespace Borm.Tests.Model.Validation;

public sealed class ValidationResultTest
{
    [Fact]
    public void Error_CreatesValidationResult_WithCorrectPropertyValues()
    {
        // Arrange
        string value = "foo";
        string entity = "entity";
        string column = "column";

        string message = Strings.ColumnValueInvalid(value);

        // Act
        ValidationResult result = ValidationResult.Error(value, entity, column);

        // Assert
        Assert.Equal(message, result.Message);
        Assert.Equal(entity, result.Entity);
        Assert.Equal(column, result.Column);
        Assert.True(result.IsError);
    }

    [Fact]
    public void Error_CreatesValidationResult_WithUserMessage()
    {
        // Arrange
        string entity = string.Empty;
        string column = string.Empty;

        string message = "foo";

        // Act
        ValidationResult result = ValidationResult.Error(message);

        // Assert
        Assert.Equal(message, result.Message);
        Assert.Equal(entity, result.Entity);
        Assert.Equal(column, result.Column);
        Assert.True(result.IsError);
    }
}
