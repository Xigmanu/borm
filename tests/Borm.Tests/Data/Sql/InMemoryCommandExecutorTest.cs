using Borm.Data.Sql;

namespace Borm.Tests.Data.Sql;

public sealed class InMemoryCommandExecutorTest
{
    [Fact]
    public void Query_ReturnsEmptyResultSet()
    {
        // Arrange
        InMemoryCommandExecutor executor = new();

        // Act
        ResultSet resultSet = executor.Query(DbCommandDefinition.Empty);

        // Assert
        Assert.Equal(0, resultSet.RowCount);
    }

    [Fact]
    public void TableExists_ReturnsFalse()
    {
        // Arrange
        InMemoryCommandExecutor executor = new();

        // Act
        bool exists = executor.TableExists(string.Empty);

        // Assert
        Assert.False(exists);
    }
}
