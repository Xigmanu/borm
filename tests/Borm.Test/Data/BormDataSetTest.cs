using System.Data;
using System.Reflection;
using Borm.Data;

namespace Borm.Tests.Data;

public sealed class BormDataSetTest
{
    [Fact]
    public void AddTable_AddsNewTableAndSubscribesEventHandlers()
    {
        // Arrange
        string dataSetName = "test_data";
        NodeDataTable table = CreateTable();
        BormDataSet dataSet = new(dataSetName);

        // Act
        dataSet.AddTable(table);
        var rowDeletedHandler = GetEventListener(table, "_onRowDeletedDelegate");
        var rowDeletingHandler = GetEventListener(table, "_onRowDeletingDelegate");

        // Assert
        Assert.Single(dataSet.Tables);
        Assert.Equal(table, dataSet.Tables[0]);
        Assert.NotNull(rowDeletedHandler);
        Assert.NotNull(rowDeletingHandler);
    }

    [Fact]
    public void Constructor_ReturnsNewInstance()
    {
        // Arrange
        string dataSetName = "test_data";

        // Act
        BormDataSet dataSet = new(dataSetName);

        // Assert
        Assert.Equal(dataSetName, dataSet.DataSetName);
    }

    [Fact]
    public void Copy_ReturnsDataSetCopyWithAllTablesAndData()
    {
        // Arrange
        string dataSetName = "test_data";
        int id = 1;
        string name = "foo";

        NodeDataTable table = CreateTable();
        table.Rows.Add(id, name);
        NodeDataTable table1 = new();
        table1.Columns.Add("id", typeof(int));
        table1.PrimaryKey = [table1.Columns[0]];
        table1.Columns.Add("fk", typeof(int));
        BormDataSet dataSet = new(dataSetName);
        dataSet.AddTable(table);
        dataSet.AddTable(table1);
        dataSet.Relations.Add("foo", table.Columns[0], table1.Columns[1]);

        // Act
        BormDataSet copy = dataSet.Copy();

        // Assert
        Assert.Equal(dataSetName, copy.DataSetName);
        Assert.Equal(dataSet.Tables.Count, copy.Tables.Count);
        Assert.Equal(dataSet.Relations.Count, copy.Relations.Count);

        foreach (NodeDataTable expectedTable in dataSet.Tables)
        {
            DataTable? actual = copy.Tables[expectedTable.TableName];

            Assert.NotNull(actual);
            Assert.IsType<NodeDataTable>(actual);
            Assert.Equal(expectedTable.TableName, actual.TableName);
            Assert.Equal(expectedTable.Rows.Count, actual.Rows.Count);
        }
    }

    [Fact]
    public void GetDeletedRowClone_ReturnsCloneOfDeletedRow_FromDataTable_WithIndex()
    {
        // Arrange
        string dataSetName = "test_data";
        NodeDataTable table = CreateTable();
        object?[] itemArray0 = [1, "foo"];
        object?[] itemArray1 = [2, "bar"];
        table.Rows.Add(itemArray0);
        table.Rows.Add(itemArray1);
        table.AcceptChanges();

        BormDataSet dataSet = new(dataSetName);
        dataSet.AddTable(table);

        table.Rows[0].Delete();
        table.Rows[1].Delete();

        // Act
        DataRow deletedClonedRow0 = dataSet.GetDeletedRowClone(table, 0);
        DataRow deletedClonedRow1 = dataSet.GetDeletedRowClone(table, 1);

        // Assert
        Assert.Equal(itemArray0, deletedClonedRow0.ItemArray);
        Assert.Equal(itemArray1, deletedClonedRow1.ItemArray);
    }

    private static NodeDataTable CreateTable()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        NodeDataTable table = new("some", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        DataColumn pkColumn = new("id", typeof(int));
        DataColumn nameColumn = new("name", typeof(string));
        table.Columns.AddRange([pkColumn, nameColumn]);
        table.PrimaryKey = [pkColumn];
        return table;
    }

    private static Delegate? GetEventListener(object obj, string field)
    {
        Type type = obj.GetType();
        return type.BaseType!.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(obj) as Delegate;
    }
}
