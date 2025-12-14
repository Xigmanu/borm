using Borm.Model;
using Borm.Model.Construction;
using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;
using Borm.Properties;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class EntityFactoryTest
{
    private static readonly ColumnValidatorFactoryContext FactoryContext = new();

    private static readonly IConfigurationValidator<IReadOnlyList<IMappable>> TestValidator =
        new TestPropertyValidator();

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenEntityTypeIsAbstract()
    {
        // Act
        Exception? exception =
            Record.Exception(() => _ = new EntityFactory<AbstractEntity>(TestValidator, FactoryContext)
            );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(
            Strings.EntityTypeCannotBeAbstract(
                typeof(AbstractEntity).FullName ?? nameof(AbstractEntity)
            ),
            exception.Message
        );
    }

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

    private abstract class AbstractEntity;

    private sealed class TestPropertyValidator : IConfigurationValidator<IReadOnlyList<IMappable>>
    {
        public void Validate(IReadOnlyList<IMappable> value)
        {
        }
    }
}