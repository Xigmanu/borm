using Borm.Data.Sql;

namespace Borm.Tests.Data.Sql;

public sealed class ResultSetTest
{
    [Fact]
    public void Current_ReturnsCurrentRow()
    {
        // Arrange
        ResultSet resultSet = new();
        Dictionary<string, object> row = new() { ["test"] = 42 };
        resultSet.AddRow(row);
        resultSet.MoveNext();

        // Act
        IReadOnlyDictionary<string, object> current = resultSet.Current;

        // Assert
        Assert.Equal(1, resultSet.RowCount);
        Assert.Single(row);
        Assert.Equal(row.Count, current.Count);
        Assert.Equal(row["test"], current["test"]);
    }

    [Fact]
    public void Current_ThrowsInvalidOperationException_WhenCursorIsLesserThanZero()
    {
        // Arrange
        ResultSet resultSet = new();

        // Act
        Exception? exception = Record.Exception(() => _ = resultSet.Current);

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}
