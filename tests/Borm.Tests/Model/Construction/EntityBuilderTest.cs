using Borm.Model;
using Borm.Model.Construction;
using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class EntityBuilderTest
{
    private static readonly IConfigurationValidator<IReadOnlyList<MappingMember>> TestValidator =
        new TestPropertyValidator();

    private static readonly ColumnValidatorFactoryContext FactoryContext = new();

    [Fact]
    public void Build_BuildsEntity_WithValidConfiguration()
    {
        // Arrange
        string name = "addresses";
        EntityBuilder<AddressEntity> builder = new(TestValidator, FactoryContext);

        // Act
        EntityInfo entityInfo = builder
            .Name(name)
            .Column(b => b.Index(0).Mapping(e => e.Id, "id").PrimaryKey())
            .Validator(new AddressEntity.Validator())
            .Build();

        // Assert
        Assert.Equal(name, entityInfo.Name);
    }

    [Fact]
    public void Column_ThrowsArgumentException_WhenColumnAlreadyExists()
    {
        // Arrange
        EntityBuilder<AddressEntity> builder = new(TestValidator, FactoryContext);

        // Act
        Exception? exception = Record.Exception(() =>
            _ = builder
                .Column(b => b.Index(1).Mapping(e => e.Id))
                .Column(b => b.Index(2).Mapping(e => e.Id))
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Name_ThrowsArgumentException_WithInvalidName(string name)
    {
        // Arrange
        EntityBuilder<AddressEntity> builder = new(TestValidator, FactoryContext);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Name(name));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Name_ThrowsArgumentNullException_WithNullName()
    {
        // Arrange
        EntityBuilder<AddressEntity> builder = new(TestValidator, FactoryContext);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Name(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Validator_ThrowsArgumentNullException_WithNullValidator()
    {
        // Arrange
        EntityBuilder<AddressEntity> builder = new(TestValidator, FactoryContext);

        // Act
        Exception? exception0 = Record.Exception(() => _ = builder.Validator((IObjectValidator<AddressEntity>)null!)
        );
        Exception? exception1 =
            Record.Exception(() => _ = builder.Validator((Func<AddressEntity, ValidationResult>)null!)
            );

        // Assert
        Assert.NotNull(exception0);
        Assert.IsType<ArgumentNullException>(exception0);
        Assert.NotNull(exception1);
        Assert.IsType<ArgumentNullException>(exception1);
    }

    private sealed class TestPropertyValidator
        : IConfigurationValidator<IReadOnlyList<MappingMember>>
    {
        public void Validate(IReadOnlyList<MappingMember> value)
        {
        }
    }
}