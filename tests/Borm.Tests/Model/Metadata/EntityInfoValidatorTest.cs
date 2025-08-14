using System.Data;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityInfoValidatorTest
{
    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void IsValid_ReturnsFalseAndInvalidOperationException_WithInvalidColumnIndex(int index)
    {
        // Arrange
        ColumnInfoCollection columns = new(
            [
                new Column(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new Column(index, "bar", "Bar", typeof(string), Constraints.AllowDbNull, null),
            ]
        );
        EntityInfo info = new("foo", typeof(object), columns);

        EntityInfoValidator validator = new([info]);

        // Act
        bool isValid = validator.IsValid(info, out Exception? exception);

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
                new Column(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new Column(
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
            [new Column(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null)]
        );

        EntityInfo info0 = new("foo", typeof(EntityA), columns0);
        EntityInfo info1 = new("foo", typeof(EntityB), columns1);

        EntityInfoValidator validator = new([info0, info1]);

        // Act
        bool isValid = validator.IsValid(info0, out Exception? exception);

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
                new Column(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new Column(1, "bar", "Bar", typeof(int), Constraints.PrimaryKey, null),
            ]
        );
        EntityInfo info = new("foo", typeof(EntityA), columns);

        EntityInfoValidator validator = new([info]);

        // Act
        bool isValid = validator.IsValid(info, out Exception? exception);

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
                new Column(
                    0,
                    "foo",
                    "Foo",
                    typeof(int),
                    Constraints.PrimaryKey | Constraints.AllowDbNull,
                    null
                ),
            ]
        );
        EntityInfo info = new("foo", typeof(EntityA), columns);

        EntityInfoValidator validator = new([info]);

        // Act
        bool isValid = validator.IsValid(info, out Exception? exception);

        // Assert
        Assert.False(isValid);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void IsValid_ReturnsFalseAndMissingPrimaryKeyException_WithNoPrimaryKey()
    {
        // Arrange
        ColumnInfoCollection columns = new(
            [new Column(0, "foo", "Foo", typeof(int), Constraints.None, null)]
        );
        EntityInfo info = new("foo", typeof(EntityA), columns);

        EntityInfoValidator validator = new([info]);

        // Act
        bool isValid = validator.IsValid(info, out Exception? exception);

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
                new Column(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new Column(
                    1,
                    "bar",
                    "Bar",
                    typeof(float?),
                    Constraints.AllowDbNull,
                    typeof(float)
                ),
            ]
        );
        EntityInfo info = new("foo", typeof(EntityA), columns0);

        EntityInfoValidator validator = new([info]);

        // Act
        bool isValid = validator.IsValid(info, out Exception? exception);

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
                new Column(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new Column(1, "bar", "Bar", references, Constraints.None, typeof(EntityB)),
            ]
        );
        ColumnInfoCollection columns1 = new(
            [new Column(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null)]
        );

        EntityInfo info0 = new("foo", typeof(EntityA), columns0);
        EntityInfo info1 = new("foo", typeof(EntityB), columns1);

        EntityInfoValidator validator = new([info0, info1]);

        // Act
        bool isValid = validator.IsValid(info0, out Exception? exception);

        // Assert
        Assert.True(isValid);
        Assert.Null(exception);
    }

#pragma warning disable S2094 // Classes should not be empty
    private static class EntityA { }

    private static class EntityB { }
#pragma warning restore S2094 // Classes should not be empty
}
