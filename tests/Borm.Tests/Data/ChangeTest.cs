using Borm.Data;
using Borm.Model.Metadata;
using Borm.Tests.Mocks;

namespace Borm.Tests.Data;

public sealed class ChangeTest
{
    private static readonly object[] Values = [1, "address", null, "city"];

    [Fact]
    public void Delete_ReturnsDeleteChange()
    {
        // Arrange
        ValueBuffer initBuffer = CreateBuffer(Values);
        long initTxId = 0;
        Change change = Change.InitChange(initBuffer, initTxId);

        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"]);
        long txId = 1;

        // Act
        Change actual = change.Delete(buffer, txId);

        // Assert
        Assert.Equal(txId, actual.WriteTxId);
        Assert.Equal(initTxId, actual.ReadTxId);
        Assert.Equal(RowAction.Delete, actual.RowAction);
        Assert.True(actual.IsWrittenToDb);
        Assert.Equal(buffer, actual.Buffer);
    }

    [Fact]
    public void InitChange_ReturnsInitialChange()
    {
        // Arrange
        ValueBuffer buffer = CreateBuffer(Values);
        long txId = 0;

        // Act
        Change change = Change.InitChange(buffer, txId);

        // Assert
        Assert.Equal(txId, change.WriteTxId);
        Assert.Equal(change.ReadTxId, change.WriteTxId);
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDb);
        Assert.Equal(buffer, change.Buffer);
    }

    [Fact]
    public void MarkAsWritten_MarksChangeAsWrittenToDataSource()
    {
        // Arrange
        ValueBuffer buffer = CreateBuffer(Values);
        long txId = 0;
        Change change = Change.InitChange(buffer, txId);

        // Act
        change.MarkAsWritten();

        // Assert
        Assert.Equal(txId, change.WriteTxId);
        Assert.Equal(change.WriteTxId, change.ReadTxId);
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDb);
    }

    [Fact]
    public void NewChange_ReturnsInsertChange()
    {
        // Arrange
        ValueBuffer buffer = CreateBuffer(Values);
        long txId = 0;

        // Act
        Change change = Change.NewChange(buffer, txId);

        // Assert
        Assert.Equal(txId, change.WriteTxId);
        Assert.Equal(change.ReadTxId, change.WriteTxId);
        Assert.Equal(RowAction.Insert, change.RowAction);
        Assert.False(change.IsWrittenToDb);
        Assert.Equal(buffer, change.Buffer);
    }

    [Fact]
    public void Update_ReturnsUpdateChange()
    {
        // Arrange
        ValueBuffer initBuffer = CreateBuffer(Values);
        long initTxId = 0;
        Change change = Change.NewChange(initBuffer, initTxId);

        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"]);
        long txId = 1;

        // Act
        Change actual = change.Update(buffer, txId);

        // Assert
        Assert.Equal(txId, actual.WriteTxId);
        Assert.Equal(initTxId, actual.ReadTxId);
        Assert.Equal(RowAction.Update, actual.RowAction);
        Assert.False(actual.IsWrittenToDb);
        Assert.Equal(buffer, actual.Buffer);
    }

    private static ValueBuffer CreateBuffer(object[] values)
    {
        Table table = TableMocks.AddressesTable;
        ValueBuffer buffer = new();

        ColumnMetadataCollection columns = table.EntityMetadata.Columns;
        for (int i = 0; i < columns.Count; i++)
        {
            buffer[columns[i]] = values[i];
        }

        return buffer;
    }
}
