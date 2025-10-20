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
        ColumnMetadataList columns = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey),
                new ColumnMetadata(index, "bar", "Bar", typeof(string), Constraints.AllowDbNull),
            ]
        );
        EntityMetadata info = new("foo", typeof(object), columns);

        EntityMetadataValidator validator = new([info]);

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
        ColumnMetadataList columns0 = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey),
                new ColumnMetadata(1, "bar", "Bar", typeof(object), Constraints.AllowDbNull)
                {
                    Reference = typeof(EntityB),
                },
            ]
        );
        ColumnMetadataList columns1 = new(
            [new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey)]
        );

        EntityMetadata info0 = new("foo", typeof(EntityA), columns0);
        EntityMetadata info1 = new("foo", typeof(EntityB), columns1);

        EntityMetadataValidator validator = new([info0, info1]);

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
        ColumnMetadataList columns = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey),
                new ColumnMetadata(1, "bar", "Bar", typeof(int), Constraints.PrimaryKey),
            ]
        );
        EntityMetadata info = new("foo", typeof(EntityA), columns);

        EntityMetadataValidator validator = new([info]);

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
        ColumnMetadataList columns = new(
            [
                new ColumnMetadata(
                    0,
                    "foo",
                    "Foo",
                    typeof(int),
                    Constraints.PrimaryKey | Constraints.AllowDbNull
                ),
            ]
        );
        EntityMetadata info = new("foo", typeof(EntityA), columns);

        EntityMetadataValidator validator = new([info]);

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
        ColumnMetadataList columns = new(
            [new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.None)]
        );
        EntityMetadata info = new("foo", typeof(EntityA), columns);

        EntityMetadataValidator validator = new([info]);

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
        ColumnMetadataList columns0 = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey),
                new ColumnMetadata(1, "bar", "Bar", typeof(float?), Constraints.AllowDbNull)
                {
                    Reference = typeof(float),
                },
            ]
        );
        EntityMetadata info = new("foo", typeof(EntityA), columns0);

        EntityMetadataValidator validator = new([info]);

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
        ColumnMetadataList columns0 = new(
            [
                new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey),
                new ColumnMetadata(1, "bar", "Bar", references, Constraints.None)
                {
                    Reference = typeof(EntityB),
                },
            ]
        );
        ColumnMetadataList columns1 = new(
            [new ColumnMetadata(0, "foo", "Foo", typeof(int), Constraints.PrimaryKey)]
        );

        EntityMetadata info0 = new("foo", typeof(EntityA), columns0);
        EntityMetadata info1 = new("foo", typeof(EntityB), columns1);

        EntityMetadataValidator validator = new([info0, info1]);

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
