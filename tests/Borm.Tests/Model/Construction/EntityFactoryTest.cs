using Borm.Model;
using Borm.Model.Construction;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class EntityFactoryTest
{
    [Fact]
    public void FromType_ReturnsEntityInfo_WithValidEntityType()
    {
        // Arrange
        // Arrange
        Type type = typeof(AddressEntity);

        // Act
        EntityInfo entity = EntityFactory.FromType(type, new AddressEntity.Validator());
        // Assert
        Assert.Equal("addresses", entity.Name);
        Assert.Equal(4, entity.Properties.Count);
        Assert.Single(entity.Constructors);
    }

    [Fact]
    public void FromType_ThrowsArgumentException_WithInvalidValidator()
    {
        // Arrange
        Type type = typeof(AddressEntity);

        // Act
        Exception? exception = Record.Exception(
            () => _ = EntityFactory.FromType(type, new PersonValidator())
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void FromType_ThrowsMemberException_WithInvalidEntityType()
    {
        // Arrange
        Type type = typeof(object);

        // Act
        Exception? exception = Record.Exception(() => _ = EntityFactory.FromType(type));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<MemberAccessException>(exception);
    }

    private sealed class PersonValidator : IEntityValidator<PersonEntity>
    {
        public void Validate(PersonEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
