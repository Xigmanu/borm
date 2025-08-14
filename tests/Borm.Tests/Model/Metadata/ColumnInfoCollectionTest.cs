using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class ColumnInfoCollectionTest
{
    [Fact]
    public void Indexer_ReturnsColumnInfo_WithColumnName()
    {
        // Arrange
        string column0Name = "foo";
        Column column0 = new(1, column0Name, "Foo", typeof(int), Constraints.None, null);
        Column column1 = new(2, "bar", "Bar", typeof(string), Constraints.AllowDbNull, null);

        ColumnInfoCollection columns = new([column0, column1]);

        // Act
        Column actual0 = columns[column0Name];

        // Assert
        Assert.Equal(column0.Name, actual0.Name);
    }
}
