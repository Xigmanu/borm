using System.Collections.ObjectModel;
using System.Data;
using Borm.Data.Sql;
using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Tests.Mocks;
using Moq;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Sql;

public sealed class ParameterBatchQueueTest
{
    [Fact]
    public void Enqueue_ShouldEnqueueValueBuffer()
    {
        // Arrange
        object[] values = [1, "address", DBNull.Value, "city"];
        IEntityMetadata metadata = EntityMetadataMockFactory.CreateMockAddressEntity();
        IValueBuffer buffer = CreateBuffer(MapValuesToColumns(values, metadata));

        ParameterBatchQueue queue = new();

        // Act
        queue.Enqueue(buffer);

        // Assert
        Assert.True(queue.HasNext());
    }

    [Fact]
    public void SetDbParameters_ShouldAssignValuesToCommandParameters()
    {
        // Arrange
        IEntityMetadata metadata = EntityMetadataMockFactory.CreateMockAddressEntity();
        IReadOnlyList<IColumnMetadata> columns = metadata.Columns;
        object[] values = [1, "address", DBNull.Value, "city"];

        IValueBuffer buffer = CreateBuffer(MapValuesToColumns(values, metadata));
        ParameterBatchQueue queue = new();
        queue.Enqueue(buffer);

        List<Mock<IDbDataParameter>> paramMocks = SetupMockParameterList(columns);

        Mock<IDataParameterCollection> collectionMock = new();
        collectionMock.Setup(pc => pc.Count).Returns(paramMocks.Count);
        collectionMock
            .Setup(pc => pc[It.IsAny<int>()])
            .Returns((int idx) => paramMocks[idx].Object);

        Mock<IDbCommand> cmdMock = new();
        cmdMock.Setup(c => c.Parameters).Returns(collectionMock.Object);

        // Act
        queue.SetParameterValues(cmdMock.Object);

        // Assert
        for (int i = 0; i < paramMocks.Count; i++)
        {
            Assert.Equal(values[i], paramMocks[i].Object.Value);
        }
    }

    private static List<Mock<IDbDataParameter>> SetupMockParameterList(
        IEnumerable<IColumnMetadata> columns
    )
    {
        List<Mock<IDbDataParameter>> parameters = [];
        foreach (IColumnMetadata column in columns)
        {
            Mock<IDbDataParameter> param = new();
            param.SetupAllProperties();
            param.Setup(p => p.ParameterName).Returns($"${column.Name}");
            parameters.Add(param);
        }

        return parameters;
    }
}
