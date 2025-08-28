using System.Data;
using Borm.Data;
using Borm.Data.Sql;
using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using static Borm.Tests.Mocks.EntityMetadataMocks;
using static Borm.Tests.Mocks.TableMocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data;

public sealed class TableTest
{
    [Fact]
    public void Delete_PendsChangeDeletion_WithExistingEntity()
    {
        // Arrange
        Table table = CreateAddressesTable();
        AddressEntity address = new(1, "address", null, "city");
        ValueBuffer valueBuffer = CreateBuffer(AddressesDummyData, table);
        long initTxId = -1;
        Change initial = Change.Initial(valueBuffer, initTxId);
        table.Tracker.PendChange(initial);
        table.Tracker.AcceptPendingChanges(initTxId);
        long txId = 0;

        // Act
        table.Delete(address, txId);
        table.AcceptPendingChanges(txId);

        // Assert
        Assert.Single(table.Tracker.Changes);

        Change actual = table.Tracker.Changes[0];
        Assert.Equal(RowAction.Delete, actual.RowAction);
        Assert.Equal(txId, actual.WriteTxId);
    }

    [Fact]
    public void Delete_ThrowsRowNotFoundException_WhenEntityIsNotFound()
    {
        // Arrange
        Table table = CreateAddressesTable();
        AddressEntity address = new(1, "address", null, "city");
        long txId = 0;

        // Act
        Exception? exception = Record.Exception(() => table.Delete(address, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<RowNotFoundException>(exception);
    }

    [Fact]
    public void GetTableSchema_ReturnsTableSchema_WithRelationalTable()
    {
        // Arrange
        Table dependency = CreateAddressesTable();
        Table table = CreatePersonsTable(dependency);

        // Act
        TableInfo schema = table.GetTableSchema();

        // Assert
        Assert.Equal(table.Name, schema.Name);
        foreach (ColumnInfo columnInfo in schema.Columns)
        {
            ColumnMetadata actual = table.EntityMetadata.Columns[columnInfo.Name];

            Assert.Equal(actual.DataType, columnInfo.DataType);
            if (actual.Constraints.HasFlag(Constraints.Unique))
            {
                Assert.True(columnInfo.IsUnique);
            }
            else
            {
                Assert.False(columnInfo.IsUnique);
            }

            if (actual.Constraints.HasFlag(Constraints.AllowDbNull))
            {
                Assert.True(columnInfo.IsNullable);
            }
            else
            {
                Assert.False(columnInfo.IsNullable);
            }

            if (schema.ForeignKeyRelations.TryGetValue(columnInfo, out TableInfo? dependencySchema))
            {
                Assert.Equal(table.Name, dependencySchema.Name);
            }
        }

        Assert.Equal(table.EntityMetadata.PrimaryKey.Name, schema.PrimaryKey.Name);
        Assert.Single(schema.ForeignKeyRelations);
    }

    [Fact]
    public void GetTableSchema_ReturnsTableSchema_WithSimpleTable()
    {
        // Arrange
        Table table = CreateAddressesTable();

        // Act
        TableInfo schema = table.GetTableSchema();

        // Assert
        Assert.Equal(table.Name, schema.Name);
        foreach (ColumnInfo columnInfo in schema.Columns)
        {
            ColumnMetadata actual = table.EntityMetadata.Columns[columnInfo.Name];

            Assert.Equal(actual.DataType, columnInfo.DataType);
            if (actual.Constraints.HasFlag(Constraints.Unique))
            {
                Assert.True(columnInfo.IsUnique);
            }
            else
            {
                Assert.False(columnInfo.IsUnique);
            }

            if (actual.Constraints.HasFlag(Constraints.AllowDbNull))
            {
                Assert.True(columnInfo.IsNullable);
            }
            else
            {
                Assert.False(columnInfo.IsNullable);
            }
        }

        Assert.Equal(table.EntityMetadata.PrimaryKey.Name, schema.PrimaryKey.Name);
        Assert.Empty(schema.ForeignKeyRelations);
    }

    [Fact]
    public void Insert_PendsNewChange_WithSimpleEntity()
    {
        // Arrange
        Table table = CreateAddressesTable();
        AddressEntity address = new(1, "address", null, "city");
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        long txId = 0;

        // Act
        table.Insert(address, txId);
        table.AcceptPendingChanges(txId);

        // Assert
        Assert.Single(table.Tracker.Changes);
        Change change = table.Tracker.Changes[0];
        Assert.Equal(RowAction.Insert, change.RowAction);
        Assert.Equal(buffer, change.Buffer);
    }

    [Fact]
    public void Insert_PendsUpdateChange_WithRelationalEntity()
    {
        // Arrange
        Table dependency = CreateAddressesTable();
        AddressEntity address = new(1, "address", null, "city");

        Table table = CreatePersonsTable(dependency);
        PersonEntity person = new(1, "name", 42.619, address);
        ValueBuffer buffer = CreateBuffer(PersonsDummyData, table);

        long txId = 0;

        // Act
        table.Insert(person, txId);
        table.AcceptPendingChanges(txId);

        // Assert
        Assert.Single(table.Tracker.Changes);
        Change change = table.Tracker.Changes[0];
        Assert.Equal(RowAction.Insert, change.RowAction);
        Assert.Equal(buffer, change.Buffer);
    }

    [Fact]
    public void Insert_ThrowsConstraintException_WhenEntityAlreadyExists()
    {
        // Arrange
        Table table = CreateAddressesTable();
        AddressEntity address = new(1, "address", null, "city");
        ValueBuffer valueBuffer = CreateBuffer(AddressesDummyData, table);
        long initTxId = -1;
        Change initial = Change.Initial(valueBuffer, initTxId);
        table.Tracker.PendChange(initial);
        table.Tracker.AcceptPendingChanges(initTxId);
        long txId = 0;

        // Act
        Exception? exception = Record.Exception(() => table.Insert(address, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConstraintException>(exception);
    }

    [Fact]
    public void Insert_ThrowsException_WhenEntityValidationFails()
    {
        // Arrange
        Table table = CreateAddressesTable();
        AddressEntity address = new(1, string.Empty, null, "city");
        long txId = 0;

        // Act
        Exception? exception = Record.Exception(() => table.Insert(address, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(table.EntityMetadata.Name, exception.Message);
    }

    [Fact]
    public void Load_DoesNothing_WhenResultSetIsEmpty()
    {
        // Arrange
        ResultSet resultSet = new();
        Table table = CreateAddressesTable();
        long initTxId = -1;

        // Act
        table.Load(resultSet, initTxId);
        table.AcceptPendingChanges(initTxId);

        // Assert
        Assert.Empty(table.Tracker.Changes);
    }

    [Fact]
    public void Load_PendsInitialChanges_WithResultSet()
    {
        // Arrange
        ResultSet resultSet = new();
        Dictionary<string, object> row = new()
        {
            ["id"] = 1,
            ["address"] = "address",
            ["address_1"] = DBNull.Value,
            ["city"] = "city",
        };
        resultSet.AddRow(row);
        long initTxId = -1;
        Table table = CreateAddressesTable();
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);

        // Act
        table.Load(resultSet, initTxId);
        table.AcceptPendingChanges(initTxId);

        // Assert
        Assert.Single(table.Tracker.Changes);

        Change change = table.Tracker.Changes[0];
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDb);
        Assert.Equal(buffer, change.Buffer);
    }

    [Fact]
    public void SelectAll_ReadsAndMaterializesEntities()
    {
        // Arrange
        Table table = CreateAddressesTable();
        AddressEntity address = new(1, "address", null, "city");
        ValueBuffer valueBuffer = CreateBuffer(AddressesDummyData, table);
        long initTxId = -1;
        Change initial = Change.Initial(valueBuffer, initTxId);
        table.Tracker.PendChange(initial);
        table.Tracker.AcceptPendingChanges(initTxId);

        // Act
        IEnumerable<object> entities = table.SelectAll();

        // Assert
        Assert.Single(entities);
        Assert.Equal(address, entities.First());
    }

    [Fact]
    public void Update_PendsNewChange_WithRelationalEntity()
    {
        // Arrange
        long initTxId = -1;
        Table dependency = CreateAddressesTable();
        AddressEntity address = new(1, "address", null, "city");
        ValueBuffer depBuffer = CreateBuffer(AddressesDummyData, dependency);
        dependency.Tracker.PendChange(Change.Initial(depBuffer, initTxId));
        dependency.Tracker.AcceptPendingChanges(initTxId);

        Table table = CreatePersonsTable(dependency);
        PersonEntity person = new(1, "no_name", 42.619, address);
        ValueBuffer buffer = CreateBuffer(PersonsDummyData, table);
        table.Tracker.PendChange(Change.Initial(buffer, initTxId));
        table.Tracker.AcceptPendingChanges(initTxId);

        long txId = 0;
        ValueBuffer expected = CreateBuffer([1, "no_name", 42.619, 1], table);

        // Act
        table.Update(person, txId);
        table.AcceptPendingChanges(txId);

        // Assert
        Assert.Single(table.Tracker.Changes);
        Change change = table.Tracker.Changes[0];
        Assert.Equal(RowAction.Update, change.RowAction);
        Assert.Equal(expected, change.Buffer);
    }

    [Fact]
    public void Update_PendsUpdateChange_WithSimpleEntity()
    {
        // Arrange
        Table table = CreateAddressesTable();
        AddressEntity address = new(1, "address", null, "not_city");
        ValueBuffer buffer = CreateBuffer([1, "address", DBNull.Value, "not_city"], table);
        ValueBuffer valueBuffer = CreateBuffer(AddressesDummyData, table);
        long initTxId = -1;
        Change initial = Change.Initial(valueBuffer, initTxId);
        table.Tracker.PendChange(initial);
        table.Tracker.AcceptPendingChanges(initTxId);
        long txId = 0;

        // Act
        table.Update(address, txId);
        table.AcceptPendingChanges(txId);

        // Assert
        Assert.Single(table.Tracker.Changes);

        Change change = table.Tracker.Changes[0];
        Assert.Equal(RowAction.Update, change.RowAction);
        Assert.Equal(buffer, change.Buffer);
    }

    [Fact]
    public void Update_ThrowsException_WhenEntityValidationFails()
    {
        // Arrange
        Table table = CreateAddressesTable();
        AddressEntity address = new(1, string.Empty, null, "city");
        long txId = 0;

        // Act
        Exception? exception = Record.Exception(() => table.Update(address, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(table.EntityMetadata.Name, exception.Message);
    }
}
