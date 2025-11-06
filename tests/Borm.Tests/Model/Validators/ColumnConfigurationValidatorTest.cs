using Borm.Model.Construction;
using Borm.Model.Validators;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Validators;

public sealed class ColumnConfigurationValidatorTest
{
    [Fact]
    public void Validate_ThrowsNotSupportedException_WhenNonForeignKeyPropertyDataTypeIsNotSupportedAnd()
    {
        // Arrange
        ColumnBuilder<AddressEntity>.Configuration configuration = new(
            "bar",
            new NullableType(typeof(object), isNullable: false),
            true,
            null
        );

        ColumnConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(configuration));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<NotSupportedException>(exception);
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WhenPrimaryKeyIsForeignKey()
    {
        // Arrange
        ColumnBuilder<AddressEntity>.Configuration configuration = new(
            "bar",
            new NullableType(typeof(int), isNullable: false),
            true,
            typeof(object)
        );

        ColumnConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(configuration));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WhenPrimaryKeyIsNullable()
    {
        // Arrange
        ColumnBuilder<AddressEntity>.Configuration configuration = new(
            "bar",
            new NullableType(typeof(int), isNullable: true),
            true,
            null
        );

        ColumnConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(configuration));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WhenPropertyNameIsNotSet()
    {
        // Arrange
        ColumnBuilder<AddressEntity>.Configuration configuration = new(null, null, false, null);

        ColumnConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(configuration));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}
