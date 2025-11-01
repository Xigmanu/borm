using Borm.Data.Sql;
using Borm.Model;

namespace Borm.Tests;

public sealed class BormConfigBuilderTest
{
    [Fact]
    public void Build_BuildsInstance_WhenAllRequiredPropertiesAreSet()
    {
        // Arrange
        BormConfig.Builder builder = new();
        builder.Model([]);
        builder.CommandExecutor(new InMemoryCommandExecutor());
        builder.CommandDefinitionFactory(new InMemoryCommandDefinitionFactory());

        // Act
        Exception? exception = Record.Exception(() => builder.Build());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenCommandDefinitionFactoryIsNull()
    {
        // Arrange
        BormConfig.Builder builder = new();
        builder.Model([]);
        builder.CommandExecutor(new InMemoryCommandExecutor());

        // Act
        Exception? exception = Record.Exception(() => builder.Build());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenCommandExecutorIsNull()
    {
        // Arrange
        BormConfig.Builder builder = new();
        builder.Model([]);

        // Act
        Exception? exception = Record.Exception(() => builder.Build());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenModelIsNull()
    {
        // Arrange
        BormConfig.Builder builder = new();

        // Act
        Exception? exception = Record.Exception(() => builder.Build());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void CommandDefinitionFactory_ThrowsArgumentNullException_WhenArgumentIsNull()
    {
        // Arrange
        BormConfig.Builder builder = new();

        // Act
        Exception? exception = Record.Exception(() => builder.CommandDefinitionFactory(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void CommandExecutor_ThrowsArgumentNullException_WhenArgumentIsNull()
    {
        // Arrange
        BormConfig.Builder builder = new();

        // Act
        Exception? exception = Record.Exception(() => builder.CommandExecutor(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void InMemory_ConfiguresInMemoryOnlyStore()
    {
        // Arrange
        BormConfig.Builder builder = new();
        List<EntityInfo> model = [];

        // Act
        BormConfig config = builder.Model(model).InMemory().Build();

        // Assert
        Assert.IsType<InMemoryCommandExecutor>(config.CommandExecutor);
        Assert.IsType<InMemoryCommandDefinitionFactory>(config.CommandDefinitionFactory);
        Assert.Equal(model, config.Model);
    }

    [Fact]
    public void Model_ThrowsArgumentNullException_WhenArgumentIsNull()
    {
        // Arrange
        BormConfig.Builder builder = new();

        // Act
        Exception? exception = Record.Exception(() => builder.Model(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }
}
