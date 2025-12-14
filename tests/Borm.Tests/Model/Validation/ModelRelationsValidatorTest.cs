using Borm.Model;
using Borm.Model.Metadata;
using Borm.Model.Validation;
using Borm.Reflection;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Model.Validation;

public sealed class ModelRelationsValidatorTest
{
    [Fact]
    public void Validate_DoesNothing_WhenNoForeignKeysExist()
    {
        // Arrange
        IMappable property = new TestProperty(
            "address",
            new NullableType(typeof(AddressEntity), false),
            new MappingInfo(0, "address", false, false, null, ReferentialAction.SetNull)
        );
        EntityInfo entity = new(null, typeof(PersonEntity), [property], [], null);
        List<EntityInfo> model = [entity];

        ModelRelationsValidator validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(model));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenModelIsNull()
    {
        // Arrange
        ModelRelationsValidator validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Validate_ThrowsEntityNotFoundException_WhenParentEntityDoesNotExist()
    {
        // Arrange
        IMappable property = new TestProperty(
            "address",
            new NullableType(typeof(AddressEntity), false),
            new MappingInfo(
                0,
                "address",
                false,
                false,
                typeof(AddressEntity),
                ReferentialAction.SetNull
            )
        );
        EntityInfo entity = new(null, typeof(PersonEntity), [property], [], null);
        List<EntityInfo> model = [entity];

        ModelRelationsValidator validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(model));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<EntityNotFoundException>(exception);
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WhenForeignKeyDataTypeIsInvalid()
    {
        // Arrange
        IMappable property0 = new TestProperty(
            "address",
            new NullableType(typeof(string), false),
            new MappingInfo(
                0,
                "address",
                false,
                false,
                typeof(AddressEntity),
                ReferentialAction.SetNull
            )
        );
        EntityInfo entity0 = new(null, typeof(PersonEntity), [property0], [], null);
        IMappable property1 = new TestProperty(
            "id",
            new NullableType(typeof(int), false),
            new MappingInfo(0, "id", true, false, null, ReferentialAction.NoAction)
        );
        EntityInfo entity1 = new(null, typeof(AddressEntity), [property1], [], null);
        List<EntityInfo> model = [entity0, entity1];

        ModelRelationsValidator validator = new();

        // Act
        Exception? exception = Record.Exception(() => validator.Validate(model));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}