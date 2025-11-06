using Borm.Model.Validators;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Validators;

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
            new NullableType(typeof(int), isNullable: false),
            new MappingInfo(
                0,
                "foo",
                IsPrimaryKey: true,
                IsUnique: false,
                null,
                Borm.Model.ReferentialAction.NoAction
            )
        );
        MappingMember property1 = new(
            "bar",
            new NullableType(typeof(int), isNullable: false),
            new MappingInfo(
                0,
                "bar",
                IsPrimaryKey: false,
                IsUnique: false,
                null,
                Borm.Model.ReferentialAction.NoAction
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
            new NullableType(typeof(int), isNullable: false),
            new MappingInfo(
                0,
                "foo",
                IsPrimaryKey: true,
                IsUnique: false,
                null,
                Borm.Model.ReferentialAction.NoAction
            )
        );
        MappingMember property1 = new(
            "foo",
            new NullableType(typeof(int), isNullable: false),
            new MappingInfo(
                1,
                "foo",
                IsPrimaryKey: false,
                IsUnique: false,
                null,
                Borm.Model.ReferentialAction.NoAction
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
            new NullableType(typeof(int), isNullable: false),
            new MappingInfo(
                0,
                "foo",
                IsPrimaryKey: true,
                IsUnique: false,
                null,
                Borm.Model.ReferentialAction.NoAction
            )
        );
        MappingMember property1 = new(
            "bar",
            new NullableType(typeof(int), isNullable: false),
            new MappingInfo(
                1,
                "bar",
                IsPrimaryKey: true,
                IsUnique: false,
                null,
                Borm.Model.ReferentialAction.NoAction
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
            new NullableType(typeof(string), isNullable: false),
            new MappingInfo(
                0,
                "foo",
                IsPrimaryKey: false,
                IsUnique: false,
                null,
                Borm.Model.ReferentialAction.NoAction
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
