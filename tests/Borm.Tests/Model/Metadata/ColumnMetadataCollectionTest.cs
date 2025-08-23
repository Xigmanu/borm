using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class ColumnMetadataCollectionTest
{
    [Fact]
    public void Indexer_ReturnsColumnInfo_WithColumnName()
    {
        // Arrange
        string column0Name = "foo";
        ColumnMetadata column0 = new(1, column0Name, "Foo", typeof(int), Constraints.None, null);
        ColumnMetadata column1 = new(2, "bar", "Bar", typeof(string), Constraints.AllowDbNull, null);

        ColumnMetadataCollection columns = new([column0, column1]);

        // Act
        ColumnMetadata actual0 = columns[column0Name];

        // Assert
        Assert.Equal(column0.Name, actual0.Name);
    }
}
