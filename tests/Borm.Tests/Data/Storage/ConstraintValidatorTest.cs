using System.Data;
using Borm.Data.Storage;
using static Borm.Tests.Mocks.TableMocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Storage;

public sealed class ConstraintValidatorTest
{
    [Fact]
    public void ValidateBuffer_ThrowsConstraintException_WhenNullConstraintIsViolated()
    {
        // Arrange
        Table table = CreateAddressesTable();
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
        Table table = CreateAddressesTable();
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
        Table table = CreateAddressesTable();
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);

        ConstraintValidator validator = new(table);

        // Act
        Exception? exception = Record.Exception(() => validator.ValidateBuffer(buffer, 0));

        // Assert
        Assert.Null(exception);
    }
}
