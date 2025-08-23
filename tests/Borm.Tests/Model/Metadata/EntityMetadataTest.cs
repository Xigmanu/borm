using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityMetadataTest
{
    [Fact]
    public void Constructor_ThrowsArgumentException_WhenColumnCollectionIsEmpty()
    {
        // Arrange
        ColumnMetadataCollection columns = new([]);

        // Act
        Exception exception = Record.Exception(
            () => _ = new EntityMetadata("foo", typeof(object), columns)
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
        ColumnMetadata pkColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnMetadataCollection columns = new([pkColumn]);
        EntityMetadata info = new("foo", typeof(object), columns);

        // Act
        ColumnMetadata actualPk = info.PrimaryKey;

        // Assert
        Assert.Equal(pkColumn.Name, actualPk.Name);
    }

    [Fact]
    public void PrimaryKey_ThrowsInvalidOperationException_WhenEntityNodeHasNoPrimaryKey()
    {
        // Arrange
        ColumnMetadata pkColumn = new(0, "id", "Id", typeof(int), Constraints.None, null);
        ColumnMetadataCollection columns = new([pkColumn]);
        EntityMetadata info = new("foo", typeof(object), columns);

        // Act
        Exception exception = Record.Exception(() => _ = info.PrimaryKey);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
