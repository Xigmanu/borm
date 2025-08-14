using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityInfoTest
{
    [Fact]
    public void Constructor_ThrowsArgumentException_WhenColumnCollectionIsEmpty()
    {
        // Arrange
        ColumnInfoCollection columns = new([]);

        // Act
        Exception exception = Record.Exception(
            () => _ = new EntityInfo("foo", typeof(object), columns)
        );

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    public void Equals_ShouldCheckIfEntityInfosAreEqual()
    {
        // Arrange
        // Act
        // Assert
    }

    [Fact]
    public void PrimaryKey_ReturnsPrimaryKeyColumn()
    {
        // Arrange
        ColumnInfo pkColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfoCollection columns = new([pkColumn]);
        EntityInfo info = new("foo", typeof(object), columns);

        // Act
        ColumnInfo actualPk = info.PrimaryKey;

        // Assert
        Assert.Equal(pkColumn.Name, actualPk.Name);
    }

    [Fact]
    public void PrimaryKey_ThrowsInvalidOperationException_WhenEntityNodeHasNoPrimaryKey()
    {
        // Arrange
        ColumnInfo pkColumn = new(0, "id", "Id", typeof(int), Constraints.None, null);
        ColumnInfoCollection columns = new([pkColumn]);
        EntityInfo info = new("foo", typeof(object), columns);

        // Act
        Exception exception = Record.Exception(() => _ = info.PrimaryKey);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
