using System.Data;
using Borm.Data.Storage;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Storage;

public sealed class ConstraintValidatorTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void ValidateBuffer_ThrowsConstraintException_WhenNullConstraintIsViolated()
    {
        // Arrange
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer([1, "address", DBNull.Value, DBNull.Value], table);

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
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        table.Tracker.PendChange(Change.NewChange(buffer, -1));
        table.AcceptPendingChanges(-1);

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
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);

        ConstraintValidator validator = new(table);

        // Act
        Exception? exception = Record.Exception(() => validator.ValidateBuffer(buffer, 0));

        // Assert
        Assert.Null(exception);
    }
}
