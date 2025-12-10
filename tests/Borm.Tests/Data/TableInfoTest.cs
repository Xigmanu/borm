using Borm.Data;

namespace Borm.Tests.Data;

public sealed class TableInfoTest
{
    private const string Name = "foo";

    public static TheoryData<object?, bool> TestData =>
        new()
        {
            { null, false },
            { string.Empty, false },
            {
                new TableInfo("bar", Columns, PrimaryKey, new Dictionary<ColumnInfo, TableInfo>()),
                false
            },
            { new TableInfo(Name, [], PrimaryKey, new Dictionary<ColumnInfo, TableInfo>()), false },
            {
                new TableInfo(Name, Columns, PrimaryKey, new Dictionary<ColumnInfo, TableInfo>()),
                true
            }
        };

    private static List<ColumnInfo> Columns => [new("bar", Name, typeof(int), true, false)];

    private static ColumnInfo PrimaryKey => new("id", Name, typeof(Guid), false, false);

    [Theory]
    [MemberData(nameof(TestData))]
    public void Equals_ChecksIfInstancesAreEqual(object? other, bool check)
    {
        // Arrange
        TableInfo table = new(Name, Columns, PrimaryKey, new Dictionary<ColumnInfo, TableInfo>());

        // Act
        bool equal = table.Equals(other);

        // Assert
        Assert.Equal(check, equal);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void GetHashCode_ReturnsCorrectHashCode(object? other, bool check)
    {
        // Arrange
        TableInfo table = new(Name, Columns, PrimaryKey, new Dictionary<ColumnInfo, TableInfo>());

        // Act
        int actual = table.GetHashCode();
        int? expected = other?.GetHashCode();

        // Assert
        if (expected.HasValue)
        {
            Assert.Equal(check, expected.Value == actual);
        }
    }
}