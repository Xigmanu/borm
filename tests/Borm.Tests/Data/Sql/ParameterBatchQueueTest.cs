using System.Data;
using Borm.Data.Sql;
using Moq;

namespace Borm.Tests.Data.Sql;

public sealed class ParameterBatchQueueTest
{
    [Fact(Skip = "Broken. Will fix later")]
    public void AddFromRow_ReadsDataRowItemArrayIntoBatchQueue()
    {
        // Arrange
        DataRow row = CreateTestRow();

        ParameterBatchQueue queue = new();

        // Act
        //queue.AddFromRow(row);

        // Assert
        Assert.Equal(1, queue.Count);
        Assert.True(queue.Next());
    }

    [Fact(Skip = "Broken. Will fix later")]
    public void SetDbParameters_ShouldAssignValuesToCommandParameters()
    {
        // Arrange
        int id = 1;
        string name = "Alice";

        ParameterBatchQueue queue = new();
        DataRow row = CreateTestRow();
        //queue.AddFromRow(row);

        Mock<IDbCommand> mockCommand = new();
        Mock<IDataParameterCollection> mockParams = new();

        Mock<IDbDataParameter> param1 = new();
        Mock<IDbDataParameter> param2 = new();

        List<IDbDataParameter> @params = [param1.Object, param2.Object];

        mockParams.Setup(p => p[It.IsAny<int>()]).Returns<int>(i => @params[i]);
        mockCommand.Setup(c => c.Parameters).Returns(mockParams.Object);

        queue.Next(); // Advance to next position

        // Act
        queue.SetParameterValues(mockCommand.Object);

        // Assert
        param1.VerifySet(p => p.Value = id, Times.Once);
        param2.VerifySet(p => p.Value = name, Times.Once);
    }

    private static DataRow CreateTestRow()
    {
        DataTable table = new();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("name", typeof(string));

        DataRow row = table.NewRow();
        row["id"] = 1;
        row["name"] = "Alice";
        table.Rows.Add(row);
        return row;
    }
}
