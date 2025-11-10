using Borm.Model;
using Borm.Model.Construction;
using Borm.Model.Validators;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class EntityFactoryTest
{
    private static readonly IValidator<IReadOnlyList<MappingMember>> TestValidator =
        new TestPropertyValidator();

    [Fact]
    public void Create_ReturnsEntityInfo_WithValidEntityType()
    {
        // Arrange
        IValidator<AddressEntity> validator = new AddressEntity.Validator();

        // Act
        EntityInfo entity = EntityFactory<AddressEntity>.Create(TestValidator, validator);

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

    private sealed class TestPropertyValidator : IValidator<IReadOnlyList<MappingMember>>
    {
        public void Validate(IReadOnlyList<MappingMember> value)
        {
        }
    }
}