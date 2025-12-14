using Borm.Data;

namespace Borm.Tests.Data;

public sealed class ColumnInfoTest
{
    private const string Table = "some";
    private const bool IsNullable = false;
    private const bool IsUnique = true;
    private const string Name = "foo";

    public static TheoryData<object?, bool> TestData =>
        new()
        {
            { null, false },
            { string.Empty, false },
            { new ColumnInfo("bar", Table, DataType, IsUnique, IsNullable), false },
            { new ColumnInfo(Name, Table, typeof(string), IsUnique, IsNullable), false },
            { new ColumnInfo(Name, Table, DataType, false, IsNullable), false },
            { new ColumnInfo(Name, Table, DataType, IsUnique, true), false },
            { new ColumnInfo(Name, Table, DataType, IsUnique, IsNullable), true }
        };

    private static Type DataType => typeof(int);

    [Fact]
    public void Constructor_InitializesNewInstanceWithPropertiesSet()
    {
        // Act
        ColumnInfo column = new(Name, Table, DataType, IsUnique, IsNullable);

        // Assert
        Assert.Equal(Name, column.Name);
        Assert.Equal(Table, column.Table);
        Assert.Equal(DataType, column.DataType);
        Assert.Equal(IsUnique, column.IsUnique);
        Assert.Equal(IsNullable, column.IsNullable);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void Equals_ChecksIfInstancesAreEqual(object? other, bool check)
    {
        // Arrange
        ColumnInfo column = new(Name, Table, DataType, IsUnique, IsNullable);

        // Act
        bool equal = column.Equals(other);

        // Assert
        Assert.Equal(check, equal);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void GetHashCode_ReturnsCorrectHashCode(object? other, bool check)
    {
        // Arrange
        ColumnInfo column = new(Name, Table, DataType, IsUnique, IsNullable);

        // Act
        int actual = column.GetHashCode();
        int? expected = other?.GetHashCode();

        // Assert
        if (expected.HasValue)
        {
            Assert.Equal(check, expected.Value == actual);
        }
    }
}