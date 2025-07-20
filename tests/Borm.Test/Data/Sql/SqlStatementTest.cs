using System.Data;
using System.Data.Common;
using Borm.Data.Sql;
using Moq;

namespace Borm.Tests.Data.Sql;

public sealed class SqlStatementTest
{
    [Fact]
    public void PrepareCommand_ShouldSetCommandTextAndAddParameters()
    {
        // Arrange
        Mock<IDbCommand> mockCommand = new();
        Mock<IDataParameterCollection> mockParams = new();

        DbParameter[] parameters = [CreateParameter("$id", 1), CreateParameter("$name", "Alice")];

        mockCommand.SetupAllProperties();
        mockCommand.Setup(c => c.Parameters).Returns(mockParams.Object);

        string sql = "INSERT INTO table VALUES($id, $name);";

        SqlStatement statement = new(sql, parameters);

        // Act
        statement.PrepareCommand(mockCommand.Object);

        // Assert
        mockCommand.VerifySet(c => c.CommandText = sql);
        mockParams.Verify(c => c.Add(It.IsAny<object>()), Times.Exactly(2));
        mockCommand.Verify(c => c.Prepare(), Times.Once);
    }

    [Fact]
    public void SetParameters_ThrowsInvalidOperationException_WhenColumnIsMissing()
    {
        // Arrange
        int id = 1;

        DataTable table = new();
        table.Columns.Add("id", typeof(int));

        DataRow row = table.NewRow();
        row["id"] = id;
        table.Rows.Add(row);

        DbParameter[] parameters = [CreateParameter("$id", null), CreateParameter("$name", null)];

        SqlStatement statement = new("some", parameters);

        // Act
        Exception exception = Record.Exception(() => statement.AddBatchValues(row));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    private static DbParameter CreateParameter(string name, object? value)
    {
        Mock<DbParameter> mockParam = new();
        mockParam.SetupAllProperties();
        mockParam.Object.ParameterName = name;
        mockParam.Object.Value = value;
        return mockParam.Object;
    }
}
