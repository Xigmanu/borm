using System.Data;
using Borm.Data;
using Borm.Model.Metadata;

namespace Borm.Tests.Data;

public sealed class NodeDataTableTest
{
    [Fact]
    public void GetParentRelation_ReturnsNull_WhenNoParentRelationExists()
    {
        // Arrange
        EntityNode node = new(
            "foo",
            typeof(object),
            new ColumnInfoCollection(
                [new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null)]
            )
        );
        EntityNode node1 = new(
            "bar",
            typeof(object),
            new ColumnInfoCollection(
                [new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null)]
            )
        );
        Table table = new("foo", node);
        table.Columns.Add("id", typeof(int));

        // Act
        DataRelation? relation = table.GetParentRelation(node1);

        // Assert
        Assert.Null(relation);
    }

    [Fact]
    public void GetParentRelation_ReturnsParentRelation()
    {
        // Arrange
        EntityNode node = new(
            "foo",
            typeof(object),
            new ColumnInfoCollection(
                [new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null)]
            )
        );
        EntityNode node1 = new(
            "bar",
            typeof(object),
            new ColumnInfoCollection(
                [new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null)]
            )
        );
        Table table = new("foo", node);
        table.Columns.Add("id", typeof(int));
        Table table1 = new("bar", node1);
        table1.Columns.Add("id", typeof(int));
        string relName = "foo_bar";

        DataSet set = new();
        set.Tables.AddRange([table, table1]);
        set.Relations.Add(relName, table1.Columns[0], table.Columns[0]);

        // Act
        DataRelation? relation = table.GetParentRelation(node1);

        // Assert
        Assert.NotNull(relation);
        Assert.Equal(relName, relation.RelationName);
    }
}
