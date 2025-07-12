using Borm.Schema.Metadata;

namespace Borm.Tests.Schema.Metadata;

public class EntityNodeTest
{
    [Fact]
    public void Constructor_ThrowsArgumentException_WhenColumnCollectionIsEmpty()
    {
        // Arrange
        ColumnInfoCollection columns = new([]);

        // Act
        Exception exception = Record.Exception(
            () => _ = new EntityNode("foo", typeof(object), columns, null)
        );

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void GetPrimaryKey_ReturnsPrimaryKeyColumn()
    {
        // Arrange
        ColumnInfo pkColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfoCollection columns = new([pkColumn]);
        EntityNode node = new("foo", typeof(object), columns, null);

        // Act
        ColumnInfo actualPk = node.GetPrimaryKey();

        // Assert
        Assert.Equal(pkColumn.Name, actualPk.Name);
    }

    [Fact]
    public void GetPrimaryKey_ThrowsInvalidOperationException_WhenEntityNodeHasNoPrimaryKey()
    {
        // Arrange
        ColumnInfo pkColumn = new(0, "id", "Id", typeof(int), Constraints.None, null);
        ColumnInfoCollection columns = new([pkColumn]);
        EntityNode node = new("foo", typeof(object), columns, null);

        // Act
        Exception exception = Record.Exception(() => _ = node.GetPrimaryKey());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
