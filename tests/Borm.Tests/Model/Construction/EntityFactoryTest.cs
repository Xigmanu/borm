using Borm.Model;
using Borm.Model.Construction;
using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class EntityFactoryTest
{
    private static readonly IConfigurationValidator<IReadOnlyList<MappingMember>> TestValidator =
        new TestPropertyValidator();

    private static readonly ColumnValidatorFactoryContext FactoryContext = new();

    [Fact]
    public void Create_ReturnsEntityInfo_WithValidEntityType()
    {
        // Arrange
        EntityFactory<AddressEntity> factory = new(TestValidator, FactoryContext);

        // Act
        EntityInfo entity = factory.Create();

        // Assert
        Assert.Equal("addresses", entity.Name);
        Assert.Equal(4, entity.Properties.Count);
        Assert.Single(entity.Constructors);
    }

    [Fact]
    public void Create_ThrowsMemberException_WithInvalidEntityType()
    {
        // Arrange
        EntityFactory<AddressEntity> factory = new(TestValidator, FactoryContext);

        // Act
        Exception? exception = Record.Exception(() => _ = factory.Create()
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