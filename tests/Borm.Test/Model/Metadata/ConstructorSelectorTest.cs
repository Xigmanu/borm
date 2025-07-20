using System.Reflection;
using Borm.Model.Metadata;
using static Borm.Tests.Mocks.ConstructorSelectorTestMocks;

namespace Borm.Tests.Model.Metadata;

public sealed class ConstructorSelectorTest
{
    [Fact]
    public void Select_ReturnsConstructorInfo_WithEntityTypeWithValidCtor()
    {
        // Arrange
        ColumnInfo idCol = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfo nameCol = new(1, "name", "Name", typeof(string), Constraints.None, null);
        ColumnInfoCollection columns = new([idCol, nameCol]);
        Type entityType = typeof(ValidCtorEntity);
        EntityConstructorSelector selector = new(columns, entityType.GetConstructors());

        // Act
        ConstructorInfo? constructor = selector.Select();

        // Assert
        Assert.NotNull(constructor);
    }

    [Fact]
    public void Select_ReturnsNull_WithEntityTypeWithDefaultCtor()
    {
        // Arrange
        ColumnInfoCollection columns = new([]);
        Type entityType = typeof(DefaultCtorEntity);
        EntityConstructorSelector selector = new(columns, entityType.GetConstructors());

        // Act
        ConstructorInfo? constructor = selector.Select();

        // Assert
        Assert.Null(constructor);
    }

    [Fact]
    public void Select_ThrowsMissingMethodException_WithEntityTypeWithInvalidCtor()
    {
        // Arrange
        ColumnInfo idCol = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfo nameCol = new(1, "name", "Name", typeof(string), Constraints.None, null);
        ColumnInfoCollection columns = new([idCol, nameCol]);
        Type entityType = typeof(InvalidCtorEntity);
        EntityConstructorSelector selector = new(columns, entityType.GetConstructors());

        // Act
        Exception exception = Record.Exception(() => _ = selector.Select());

        // Assert
        Assert.IsType<MissingMethodException>(exception);
    }

    [Fact]
    public void Select_ThrowsMissingMethodException_WithEntityTypeWithUnEqualParameterCountCtor()
    {
        // Arrange
        ColumnInfo idCol = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfo nameCol = new(1, "name", "Name", typeof(string), Constraints.None, null);
        ColumnInfoCollection columns = new([idCol, nameCol]);
        Type entityType = typeof(UnEqualParameterCountCtorEntity);
        EntityConstructorSelector selector = new(columns, entityType.GetConstructors());

        // Act
        Exception exception = Record.Exception(() => _ = selector.Select());

        // Assert
        Assert.IsType<MissingMethodException>(exception);
    }
}
