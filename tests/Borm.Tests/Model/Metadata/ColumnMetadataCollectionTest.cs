using Borm.Model;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Tests.Model.Metadata;

public sealed class ColumnMetadataCollectionTest
{
    [Fact]
    public void Indexer_ReturnsColumnInfo_WithColumnName()
    {
        // Arrange
        string column0Name = "foo";
        ColumnMetadata column0 = new(
            1,
            column0Name,
            "Foo",
            new NullableType(typeof(int), isNullable: false),
            Constraints.None
        );
        ColumnMetadata column1 = new(
            2,
            "bar",
            "Bar",
            new NullableType(typeof(string), isNullable: true),
            Constraints.AllowDbNull
        );

        ColumnMetadataList columns = new([column0, column1]);

        // Act
        IColumnMetadata actual0 = columns[column0Name];

        // Assert
        Assert.Equal(column0.Name, actual0.Name);
    }
}
