using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using System.Data;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage;

public sealed class ConstraintValidatorTest
{
    private readonly ITableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void ValidateBuffer_ThrowsConstraintException_WhenNullConstraintIsViolated()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", DBNull.Value, DBNull.Value], table.Metadata.Columns)
        );

        ConstraintValidator validator = new(table);

        // Act
        Exception? exception = Record.Exception(() => validator.ValidateBuffer(buffer, 0));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConstraintException>(exception);
    }

    [Fact]
    public void ValidateBuffer_ThrowsConstraintException_WhenUniqueConstraintIsViolated()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table.Metadata.Columns)
        );
        table.Tracker.PendChange(ChangeFactory.NewChange(buffer, -1));
        table.Tracker.AcceptPendingChanges(-1);

        ConstraintValidator validator = new(table);

        // Act
        Exception? exception = Record.Exception(() => validator.ValidateBuffer(buffer, 0));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConstraintException>(exception);
    }

    [Fact]
    public void ValidateBuffer_ThrowsNothing_WhenNoConstraintsAreViolated()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table.Metadata.Columns)
        );

        ConstraintValidator validator = new(table);

        // Act
        Exception? exception = Record.Exception(() => validator.ValidateBuffer(buffer, 0));

        // Assert
        Assert.Null(exception);
    }
}