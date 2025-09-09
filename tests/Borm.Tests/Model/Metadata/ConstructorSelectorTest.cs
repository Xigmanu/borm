using System.Reflection;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class ConstructorSelectorTest
{
    [Fact]
    public void Select_ReturnsConstructorInfo_WithEntityTypeWithValidCtor()
    {
        // Arrange
        ColumnMetadata idCol = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey);
        ColumnMetadata nameCol = new(1, "name", "Name", typeof(string), Constraints.None);
        ColumnMetadataCollection columns = new([idCol, nameCol]);
        Type entityType = typeof(ConstructorSelectorTestMocks.ValidCtorEntity);
        ConstructorSelector selector = new(columns, entityType.GetConstructors());

        // Act
        ConstructorInfo? constructor = selector.Select();

        // Assert
        Assert.NotNull(constructor);
    }

    [Fact]
    public void Select_ReturnsNull_WithEntityTypeWithDefaultCtor()
    {
        // Arrange
        ColumnMetadataCollection columns = new([]);
        Type entityType = typeof(ConstructorSelectorTestMocks.DefaultCtorEntity);
        ConstructorSelector selector = new(columns, entityType.GetConstructors());

        // Act
        ConstructorInfo? constructor = selector.Select();

        // Assert
        Assert.Null(constructor);
    }

    [Fact]
    public void Select_ThrowsMissingMethodException_WithEntityTypeWithInvalidCtor()
    {
        // Arrange
        ColumnMetadata idCol = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnMetadata nameCol = new(1, "name", "Name", typeof(string), Constraints.None, null);
        ColumnMetadataCollection columns = new([idCol, nameCol]);
        Type entityType = typeof(ConstructorSelectorTestMocks.InvalidCtorEntity);
        ConstructorSelector selector = new(columns, entityType.GetConstructors());

        // Act
        Exception exception = Record.Exception(() => _ = selector.Select());

        // Assert
        Assert.IsType<MissingMethodException>(exception);
    }

    [Fact]
    public void Select_ThrowsMissingMethodException_WithEntityTypeWithUnEqualParameterCountCtor()
    {
        // Arrange
        ColumnMetadata idCol = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnMetadata nameCol = new(1, "name", "Name", typeof(string), Constraints.None, null);
        ColumnMetadataCollection columns = new([idCol, nameCol]);
        Type entityType = typeof(ConstructorSelectorTestMocks.UnEqualParameterCountCtorEntity);
        ConstructorSelector selector = new(columns, entityType.GetConstructors());

        // Act
        Exception exception = Record.Exception(() => _ = selector.Select());

        // Assert
        Assert.IsType<MissingMethodException>(exception);
    }

    private static class ConstructorSelectorTestMocks
    {
#pragma warning disable S2094, CS9113
        public sealed class DefaultCtorEntity;

        public sealed class InvalidCtorEntity(int id, string foo);

        public sealed class UnEqualParameterCountCtorEntity(int id);

        public sealed class ValidCtorEntity(int id, string name);
#pragma warning restore S2094, CS9113
    }
}
