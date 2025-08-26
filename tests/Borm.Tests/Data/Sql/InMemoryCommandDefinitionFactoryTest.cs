using System.Diagnostics;
using Borm.Data;
using Borm.Data.Sql;

namespace Borm.Tests.Data.Sql;

public sealed class InMemoryCommandDefinitionFactoryTest
{
    [Fact]
    public void CreateTable_ReturnsEmptyCommandDefinition()
    {
        // Arrange
        TableInfo schema = CreateTestTableSchema();
        InMemoryCommandDefinitionFactory factory = new();

        // Act
        DbCommandDefinition definition = factory.CreateTable(schema);

        // Assert
        Assert.Equal(DbCommandDefinition.Empty, definition);
    }

    [Fact]
    public void Delete_ReturnsEmptyCommandDefinition()
    {
        // Arrange
        TableInfo schema = CreateTestTableSchema();
        InMemoryCommandDefinitionFactory factory = new();

        // Act
        DbCommandDefinition definition = factory.Delete(schema);

        // Assert
        Assert.Equal(DbCommandDefinition.Empty, definition);
    }

    [Fact]
    public void Insert_ReturnsEmptyCommandDefinition()
    {
        // Arrange
        TableInfo schema = CreateTestTableSchema();
        InMemoryCommandDefinitionFactory factory = new();

        // Act
        DbCommandDefinition definition = factory.Insert(schema);

        // Assert
        Assert.Equal(DbCommandDefinition.Empty, definition);
    }

    [Fact]
    public void SelectAll_ReturnsEmptyCommandDefinition()
    {
        // Arrange
        TableInfo schema = CreateTestTableSchema();
        InMemoryCommandDefinitionFactory factory = new();

        // Act
        DbCommandDefinition definition = factory.SelectAll(schema);

        // Assert
        Assert.Equal(DbCommandDefinition.Empty, definition);
    }

    [Fact]
    public void Update_ReturnsEmptyCommandDefinition()
    {
        // Arrange
        TableInfo schema = CreateTestTableSchema();
        InMemoryCommandDefinitionFactory factory = new();

        // Act
        DbCommandDefinition definition = factory.Update(schema);

        // Assert
        Assert.Equal(DbCommandDefinition.Empty, definition);
    }

    [DebuggerStepThrough]
    private static TableInfo CreateTestTableSchema()
    {
        return new TableInfo(
            string.Empty,
            [],
            new ColumnInfo(string.Empty, typeof(int), false, false),
            new Dictionary<ColumnInfo, TableInfo>()
        );
    }
}
