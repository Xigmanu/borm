using Borm.Model;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityMetadataTest
{
    [Fact]
    public void Constructor_ThrowsArgumentException_WhenColumnCollectionIsEmpty()
    {
        // Arrange
        ColumnMetadataList columns = new([]);

        // Act
        Exception exception = Record.Exception(
            () => _ = new EntityMetadata("foo", typeof(object), columns)
        );

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenEntityNamesAreNotEqual()
    {
        // Arrange
        ColumnMetadata pkColumn = new(
            0,
            "id",
            "Id",
            new NullableType(typeof(int), isNullable: false),
            Constraints.PrimaryKey
        );
        ColumnMetadataList columns = new([pkColumn]);
        EntityMetadata metadata = new("foo", typeof(object), columns);
        EntityMetadata other = new("bar", typeof(object), columns);

        // Act
        bool equals = metadata.Equals(other);

        // Assert
        Assert.False(equals);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Equals_ReturnsFalse_WhenOtherIsOfDifferentTypeOrNull(object? other)
    {
        // Arrange
        ColumnMetadata pkColumn = new(
            0,
            "id",
            "Id",
            new NullableType(typeof(int), isNullable: false),
            Constraints.PrimaryKey
        );
        ColumnMetadataList columns = new([pkColumn]);
        EntityMetadata metadata = new("foo", typeof(object), columns);

        // Act
        bool equals = metadata.Equals(other);

        // Assert
        Assert.False(equals);
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenEntityNamesAreEqual()
    {
        // Arrange
        ColumnMetadata pkColumn = new(
            0,
            "id",
            "Id",
            new NullableType(typeof(int), isNullable: false),
            Constraints.PrimaryKey
        );
        ColumnMetadataList columns = new([pkColumn]);
        EntityMetadata metadata = new("foo", typeof(object), columns);
        EntityMetadata other = new("foo", typeof(object), columns);

        // Act
        bool equals = metadata.Equals(other);

        // Assert
        Assert.True(equals);
    }

    [Fact]
    public void PrimaryKey_ReturnsPrimaryKeyColumn()
    {
        // Arrange
        ColumnMetadata pkColumn = new(
            0,
            "id",
            "Id",
            new NullableType(typeof(int), isNullable: false),
            Constraints.PrimaryKey
        );
        ColumnMetadataList columns = new([pkColumn]);
        EntityMetadata info = new("foo", typeof(object), columns);

        // Act
        IColumnMetadata actualPk = info.PrimaryKey;

        // Assert
        Assert.Equal(pkColumn.Name, actualPk.Name);
    }

    [Fact]
    public void PrimaryKey_ThrowsInvalidOperationException_WhenEntityNodeHasNoPrimaryKey()
    {
        // Arrange
        ColumnMetadata pkColumn = new(
            0,
            "id",
            "Id",
            new NullableType(typeof(int), isNullable: false),
            Constraints.None
        );
        ColumnMetadataList columns = new([pkColumn]);
        EntityMetadata info = new("foo", typeof(object), columns);

        // Act
        Exception exception = Record.Exception(() => _ = info.PrimaryKey);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
