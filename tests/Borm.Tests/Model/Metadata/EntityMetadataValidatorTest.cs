using System.Data;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityMetadataValidatorTest
{
    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void IsValid_ReturnsFalseAndInvalidOperationException_WithInvalidColumnIndex(int index)
    {
        // Arrange
        ColumnMetadataCollection columns = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnMetadata(index, "bar", "Bar", typeof(string), Constraints.AllowDbNull, null),
            ]
        );
        EntityMetadata info = new("foo", typeof(object), columns);

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
        ColumnMetadataCollection columns0 = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnMetadata(
                    1,
                    "bar",
                    "Bar",
                    typeof(object),
                    Constraints.AllowDbNull,
                    typeof(EntityB)
                ),
            ]
        );
        ColumnMetadataCollection columns1 = new(
            [new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null)]
        );

        EntityMetadata info0 = new("foo", typeof(EntityA), columns0);
        EntityMetadata info1 = new("foo", typeof(EntityB), columns1);

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
        ColumnMetadataCollection columns = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnMetadata(1, "bar", "Bar", typeof(int), Constraints.PrimaryKey, null),
            ]
        );
        EntityMetadata info = new("foo", typeof(EntityA), columns);

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
        ColumnMetadataCollection columns = new(
            [
                new ColumnMetadata(
                    0,
                    "foo",
                    "Foo",
                    typeof(int),
                    Constraints.PrimaryKey | Constraints.AllowDbNull,
                    null
                ),
            ]
        );
        EntityMetadata info = new("foo", typeof(EntityA), columns);

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
        ColumnMetadataCollection columns = new(
            [new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.None, null)]
        );
        EntityMetadata info = new("foo", typeof(EntityA), columns);

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
        ColumnMetadataCollection columns0 = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnMetadata(
                    1,
                    "bar",
                    "Bar",
                    typeof(float?),
                    Constraints.AllowDbNull,
                    typeof(float)
                ),
            ]
        );
        EntityMetadata info = new("foo", typeof(EntityA), columns0);

        EntityInfoValidator validator = new([info]);

        // Act
        bool isValid = validator.IsValid(info, out Exception? exception);

        // Assert
        Assert.False(isValid);
        Assert.IsType<EntityNotFoundException>(exception);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(EntityB))]
    public void IsValid_ReturnsTrue_WithValidEntityNode(Type references)
    {
        // Arrange
        ColumnMetadataCollection columns0 = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null),
                new ColumnMetadata(1, "bar", "Bar", references, Constraints.None, typeof(EntityB)),
            ]
        );
        ColumnMetadataCollection columns1 = new(
            [new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey, null)]
        );

        EntityMetadata info0 = new("foo", typeof(EntityA), columns0);
        EntityMetadata info1 = new("foo", typeof(EntityB), columns1);

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
