using Borm.Model;
using Borm.Model.Construction;
using Borm.Model.Validation;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class EntityFactoryTest
{
    private static readonly IConfigurationValidator<IReadOnlyList<MappingMember>> TestValidator =
        new TestPropertyValidator();

    [Fact]
    public void Create_ReturnsEntityInfo_WithValidEntityType()
    {
        // Arrange
        IObjectValidator<AddressEntity> validator = new AddressEntity.Validator();

        // Act
        EntityInfo entity = EntityFactory<AddressEntity>.Create(TestValidator, o => validator.Validate((AddressEntity)o));

        // Assert
        Assert.Equal("addresses", entity.Name);
        Assert.Equal(4, entity.Properties.Count);
        Assert.Single(entity.Constructors);
    }

    [Fact]
    public void Create_ThrowsMemberException_WithInvalidEntityType()
    {
        // Act
        Exception? exception = Record.Exception(() => _ = EntityFactory<object>.Create(TestValidator)
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<MemberAccessException>(exception);
    }

    private sealed class TestPropertyValidator : IConfigurationValidator<IReadOnlyList<MappingMember>>
    {
        public void Validate(IReadOnlyList<MappingMember> value)
        {
        }
    }
}