using System.Data;
using Borm.Schema.Metadata;

namespace Borm.Tests.Schema.Metadata;

public class EntityNodeValidatorTest
{
    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void IsValid_ReturnsFalseAndInvalidOperationException_WithInvalidColumnIndex(int index)
    {
        // Arrange
        ColumnInfoCollection columns = new(
            [
                new ColumnInfo(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnInfo(index, "bar", "Bar", typeof(string), Constraints.AllowDbNull, null),
            ]
        );
        EntityNode node = new("foo", typeof(object), columns, null);

        EntityNodeValidator validator = new([node]);

        // Act
        bool isValid = validator.IsValid(node, out Exception? exception);

        // Assert
        Assert.False(isValid);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void IsValid_ReturnsFalseAndInvalidOperationException_WithInvalidForeignKeyDataType()
    {
        // Arrange
        ColumnInfoCollection columns0 = new(
            [
                new ColumnInfo(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnInfo(
                    1,
                    "bar",
                    "Bar",
                    typeof(object),
                    Constraints.AllowDbNull,
                    typeof(EntityB)
                ),
            ]
        );
        ColumnInfoCollection columns1 = new(
            [new ColumnInfo(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null)]
        );

        EntityNode node0 = new("foo", typeof(EntityA), columns0, null);
        EntityNode node1 = new("foo", typeof(EntityB), columns1, null);

        EntityNodeValidator validator = new([node0, node1]);

        // Act
        bool isValid = validator.IsValid(node0, out Exception? exception);

        // Assert
        Assert.False(isValid);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void IsValid_ReturnsFalseAndInvalidOperationException_WithMultiplePrimaryKeys()
    {
        // Arrange
        ColumnInfoCollection columns = new(
            [
                new ColumnInfo(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnInfo(1, "bar", "Bar", typeof(int), Constraints.PrimaryKey, null),
            ]
        );
        EntityNode node = new("foo", typeof(EntityA), columns, null);

        EntityNodeValidator validator = new([node]);

        // Act
        bool isValid = validator.IsValid(node, out Exception? exception);

        // Assert
        Assert.False(isValid);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void IsValid_ReturnsFalseAndInvalidOperationException_WithNullablePrimaryKey()
    {
        // Arrange
        ColumnInfoCollection columns = new(
            [
                new ColumnInfo(
                    0,
                    "foo",
                    "Foo",
                    typeof(int),
                    Constraints.PrimaryKey | Constraints.AllowDbNull,
                    null
                ),
            ]
        );
        EntityNode node = new("foo", typeof(EntityA), columns, null);

        EntityNodeValidator validator = new([node]);

        // Act
        bool isValid = validator.IsValid(node, out Exception? exception);

        // Assert
        Assert.False(isValid);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void IsValid_ReturnsFalseAndMissingPrimaryKeyException_WithNoPrimaryKey()
    {
        // Arrange
        ColumnInfoCollection columns = new(
            [new ColumnInfo(0, "foo", "Foo", typeof(int), Constraints.None, null)]
        );
        EntityNode node = new("foo", typeof(EntityA), columns, null);

        EntityNodeValidator validator = new([node]);

        // Act
        bool isValid = validator.IsValid(node, out Exception? exception);

        // Assert
        Assert.False(isValid);
        Assert.IsType<MissingPrimaryKeyException>(exception);
    }

    [Fact]
    public void IsValid_ReturnsFalseAndNodeNotFoundException_WithMissingForeignKeyNode()
    {
        // Arrange
        ColumnInfoCollection columns0 = new(
            [
                new ColumnInfo(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnInfo(
                    1,
                    "bar",
                    "Bar",
                    typeof(float?),
                    Constraints.AllowDbNull,
                    typeof(float)
                ),
            ]
        );
        EntityNode node0 = new("foo", typeof(EntityA), columns0, null);

        EntityNodeValidator validator = new([node0]);

        // Act
        bool isValid = validator.IsValid(node0, out Exception? exception);

        // Assert
        Assert.False(isValid);
        Assert.IsType<NodeNotFoundException>(exception);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(EntityB))]
    public void IsValid_ReturnsTrue_WithValidEntityNode(Type references)
    {
        // Arrange
        ColumnInfoCollection columns0 = new(
            [
                new ColumnInfo(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnInfo(1, "bar", "Bar", references, Constraints.None, typeof(EntityB)),
            ]
        );
        ColumnInfoCollection columns1 = new(
            [new ColumnInfo(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null)]
        );

        EntityNode node0 = new("foo", typeof(EntityA), columns0, null);
        EntityNode node1 = new("foo", typeof(EntityB), columns1, null);

        EntityNodeValidator validator = new([node0, node1]);

        // Act
        bool isValid = validator.IsValid(node0, out Exception? exception);

        // Assert
        Assert.True(isValid);
        Assert.Null(exception);
    }

#pragma warning disable S2094 // Classes should not be empty
    private static class EntityA { }

    private static class EntityB { }
#pragma warning restore S2094 // Classes should not be empty
}
