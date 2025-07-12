using System.Data;
using Borm.Extensions;

namespace Borm.Tests.Extensions;

public class DataRelationExtensionsTest
{
    [Fact]
    public void Copy_ReturnsCopyOfDataRelation_WithOriginalDataRelationAndTargetDataSet()
    {
        // Arrange
        DataTable table0 = new("table0");
        table0.Columns.Add("parent", typeof(int));
        DataTable table1 = new("table1");
        table1.Columns.Add("child", typeof(int));

        DataSet dataSet = new();
        dataSet.Tables.AddRange([table0, table1]);
        DataRelation relation = new("relation", table0.Columns[0], table1.Columns[0]);
        dataSet.Relations.Add(relation);

        DataSet dataSetCopy = new();
        dataSetCopy.Tables.AddRange([table0.Copy(), table1.Copy()]);

        // Act
        DataRelation copyRelation = relation.Copy(dataSetCopy);

        // Assert
        Assert.Equal(relation.RelationName, copyRelation.RelationName);

        Assert.Equal(relation.ParentTable.TableName, copyRelation.ParentTable.TableName);
        Assert.Equal(relation.ChildTable.TableName, copyRelation.ChildTable.TableName);

        Assert.Equal(relation.ParentColumns.Length, copyRelation.ParentColumns.Length);
        Assert.Equal(relation.ChildColumns.Length, copyRelation.ChildColumns.Length);

        Assert.Equal(
            relation.ParentColumns[0].ColumnName,
            copyRelation.ParentColumns[0].ColumnName
        );
        Assert.Equal(relation.ChildColumns[0].ColumnName, copyRelation.ChildColumns[0].ColumnName);
    }

    [Fact]
    public void Copy_ThrowsInvalidOperationException_WhenRelationColumnsDoNotExistInDataSetCopy()
    {
        // Arrange
        DataTable table0 = new("table0");
        table0.Columns.Add("parent", typeof(int));
        DataTable table1 = new("table1");
        table1.Columns.Add("child", typeof(int));

        DataSet dataSet = new();
        dataSet.Tables.AddRange([table0, table1]);
        DataRelation relation = new("relation", table0.Columns[0], table1.Columns[0]);
        dataSet.Relations.Add(relation);

        DataSet dataSetCopy = new();
        DataTable table1Copy = table1.Copy();
        table1Copy.Columns.Add("some", typeof(string));
        table1Copy.Columns.RemoveAt(0);
        dataSetCopy.Tables.AddRange([table0.Copy(), table1Copy]);

        // Act
        Exception exception = Record.Exception(() => _ = relation.Copy(dataSetCopy));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
