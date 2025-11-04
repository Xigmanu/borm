using Borm.Model;
using Borm.Model.Construction;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class EntityBuilderTest
{
    [Fact]
    public void Build_BuildsEntity_WithValidConfiguration()
    {
        // Arrange
        string name = "addresses";
        EntityBuilder<AddressEntity> builder = new();

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
    public void Build_ThrowsInvalidOperationException_WhenNoColumnsConfigured()
    {
        // Arrange
        EntityBuilder<AddressEntity> builder = new();

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Build());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Column_ThrowsArgumentException_WhenColumnAlreadyExists()
    {
        // Arrange
        EntityBuilder<AddressEntity> builder = new();

        // Act
        Exception? exception = Record.Exception(
            () =>
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
        EntityBuilder<AddressEntity> builder = new();

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
        EntityBuilder<AddressEntity> builder = new();

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
        EntityBuilder<AddressEntity> builder = new();

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Validator(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }
}
