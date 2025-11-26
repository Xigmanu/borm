using Borm.Model.Validation;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Validation;

public sealed class ValidationResultTest
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Error_ThrowsArgumentException_WhenMemberExpressionStringIsNullOrWhiteSpace(
        string? expression
    )
    {
        // Arrange
        string? message = null;
        object? value = null;

        // Act
#pragma warning disable S3236
        Exception? exception = Record.Exception(
            () => _ = ValidationResult.Error(value, message, expression)
        );
#pragma warning restore S3236

        //Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }
}
