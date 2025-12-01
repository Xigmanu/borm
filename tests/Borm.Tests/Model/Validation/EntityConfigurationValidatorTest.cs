using Borm.Model;
using Borm.Model.Validation;
using Borm.Reflection;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Model.Validation;

public sealed class EntityConfigurationValidatorTest
{
    [Fact]
    public void Validate_ThrowsArgumentException_WithEmptyPropertyList()
    {
        // Arrange
        IReadOnlyList<IMappable> properties = [];
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
        IReadOnlyList<IMappable> properties = null!;
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
        IMappable property0 = new TestProperty(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(0, "foo", true, false, null, ReferentialAction.NoAction)
        );
        IMappable property1 = new TestProperty(
            "bar",
            new NullableType(typeof(int), false),
            new MappingInfo(0, "bar", false, false, null, ReferentialAction.NoAction)
        );
        IReadOnlyList<IMappable> properties = [property0, property1];
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
        IMappable property0 = new TestProperty(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(0, "foo", true, false, null, ReferentialAction.NoAction)
        );
        IMappable property1 = new TestProperty(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(1, "foo", false, false, null, ReferentialAction.NoAction)
        );
        IReadOnlyList<IMappable> properties = [property0, property1];
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
        IMappable property0 = new TestProperty(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(0, "foo", true, false, null, ReferentialAction.NoAction)
        );
        IMappable property1 = new TestProperty(
            "bar",
            new NullableType(typeof(int), false),
            new MappingInfo(1, "bar", true, false, null, ReferentialAction.NoAction)
        );
        IReadOnlyList<IMappable> properties = [property0, property1];
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
        IMappable property = new TestProperty(
            "foo",
            new NullableType(typeof(string), false),
            new MappingInfo(0, "foo", false, false, null, ReferentialAction.NoAction)
        );
        IReadOnlyList<IMappable> properties = [property];
        EntityConfigurationValidator<AddressEntity> validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(properties));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}