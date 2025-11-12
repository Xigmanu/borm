using Borm.Model.Validation;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Validation;

public sealed class ValidationResultTest
{
    [Fact]
    public void Error_ReturnsErrorResult_WithValidArguments()
    {
        // Arrange
        AddressEntity entity = new(1, "foo", null, "bar");

        // Act
        ValidationResult result = ValidationResult.Error(entity.Address);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(nameof(entity.Address), result.MemberName);
        Assert.Null(result.Message);
    }

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

    [Fact]
    public void Ok_HasDefaultProperties()
    {
        // Arrange
        string? message = null;
        string memberName = string.Empty;

        // Act
        ValidationResult result = ValidationResult.Ok;

        // Assert
        Assert.Equal(message, result.Message);
        Assert.Equal(memberName, result.MemberName);
        Assert.False(result.IsError);
    }
}
