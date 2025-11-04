using Borm.Model;
using Borm.Model.Construction;
using Borm.Model.Validators;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class EntityFactoryTest
{
    [Fact]
    public void Create_ReturnsEntityInfo_WithValidEntityType()
    {
        // Arrange
        IEntityValidator<AddressEntity> validator = new AddressEntity.Validator();

        // Act
        EntityInfo entity = EntityFactory<AddressEntity>.Create(validator);

        // Assert
        Assert.Equal("addresses", entity.Name);
        Assert.Equal(4, entity.Properties.Count);
        Assert.Single(entity.Constructors);
    }

    [Fact]
    public void Create_ThrowsMemberException_WithInvalidEntityType()
    {
        // Act
        Exception? exception = Record.Exception(() => _ = EntityFactory<object>.Create());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<MemberAccessException>(exception);
    }
}
