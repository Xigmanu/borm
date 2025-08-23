using Borm.Data;

namespace Borm.Tests.Data;

public sealed class TransactionIdMismatchExceptionTest
{
    [Fact]
    public void Constructor_SetsTransactionIds()
    {
        // Arrange
        long currentTxId = 42;
        long incomingTxId = 619;

        // Act
        TransactionIdMismatchException actual = new(string.Empty, currentTxId, incomingTxId);

        // Assert
        Assert.Equal(currentTxId, actual.CurrentTxId);
        Assert.Equal(incomingTxId, actual.IncomingTxId);
    }
}
