using System.Data;
using Borm.Data;
using Borm.Data.Sql;
using Borm.Model.Metadata;
using Moq;

namespace Borm.Tests.Data.Sql;

public sealed class ParameterBatchQueueTest
{
    [Fact]
    public void Enqueue_ShouldEnqueueValueBuffer()
    {
        // Arrange
        ValueBuffer buffer = CreateTestBuffer();

        ParameterBatchQueue queue = new();

        // Act
        queue.Enqueue(buffer);

        // Assert
        Assert.True(queue.HasNext());
    }

    [Fact(Skip = "Broken for now")]
    public void SetDbParameters_ShouldAssignValuesToCommandParameters()
    {
        // Arrange
        int id = 1;

        ValueBuffer buffer = CreateTestBuffer();
        ParameterBatchQueue queue = new();
        queue.Enqueue(buffer);

        Mock<IDbCommand> mockCommand = new();
        Mock<IDataParameterCollection> mockParams = new();

        Mock<IDbDataParameter> param1 = new();

        List<IDbDataParameter> @params = [param1.Object];

        mockParams.Setup(p => p[It.IsAny<int>()]).Returns<int>(i => @params[i]);
        mockCommand.Setup(c => c.Parameters).Returns(mockParams.Object);

        // Act
        queue.SetParameterValues(mockCommand.Object);

        // Assert
        param1.VerifySet(p => p.Value = id, Times.Once);
    }

    private static ValueBuffer CreateTestBuffer()
    {
        ValueBuffer buffer = new();
        Column idColumn = new(
            0,
            "id",
            "Id",
            typeof(int),
            Borm.Model.Constraints.PrimaryKey,
            null
        );
        buffer[idColumn] = 1;
        return buffer;
    }
}
