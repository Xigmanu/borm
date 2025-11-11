using Borm.Model;
using Borm.Model.Validation;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Validation;

public sealed class EntityConfigurationValidatorTest
{
    [Fact]
    public void Validate_ThrowsArgumentException_WithEmptyPropertyList()
    {
        // Arrange
        IReadOnlyList<MappingMember> properties = [];
        EntityConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(properties));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WithNullArgument()
    {
        // Arrange
        IReadOnlyList<MappingMember> properties = null!;
        EntityConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(properties));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WithDuplicateColumnIndexes()
    {
        // Arrange
        MappingMember property0 = new(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(
                0,
                "foo",
                true,
                false,
                null,
                ReferentialAction.NoAction
            )
        );
        MappingMember property1 = new(
            "bar",
            new NullableType(typeof(int), false),
            new MappingInfo(
                0,
                "bar",
                false,
                false,
                null,
                ReferentialAction.NoAction
            )
        );
        IReadOnlyList<MappingMember> properties = [property0, property1];
        EntityConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(properties));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WithDuplicateColumnNames()
    {
        // Arrange
        MappingMember property0 = new(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(
                0,
                "foo",
                true,
                false,
                null,
                ReferentialAction.NoAction
            )
        );
        MappingMember property1 = new(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(
                1,
                "foo",
                false,
                false,
                null,
                ReferentialAction.NoAction
            )
        );
        IReadOnlyList<MappingMember> properties = [property0, property1];
        EntityConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(properties));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WithDuplicatePrimaryKeys()
    {
        // Arrange
        MappingMember property0 = new(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(
                0,
                "foo",
                true,
                false,
                null,
                ReferentialAction.NoAction
            )
        );
        MappingMember property1 = new(
            "bar",
            new NullableType(typeof(int), false),
            new MappingInfo(
                1,
                "bar",
                true,
                false,
                null,
                ReferentialAction.NoAction
            )
        );
        IReadOnlyList<MappingMember> properties = [property0, property1];
        EntityConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(properties));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WithNoPrimaryKeys()
    {
        // Arrange
        MappingMember property = new(
            "foo",
            new NullableType(typeof(string), false),
            new MappingInfo(
                0,
                "foo",
                false,
                false,
                null,
                ReferentialAction.NoAction
            )
        );
        IReadOnlyList<MappingMember> properties = [property];
        EntityConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(properties));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}