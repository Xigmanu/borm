using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class ColumnMetadataTest
{
    [Fact]
    public void Constructor_ShouldAssignPropertyNameAndColumnName_WithValidCtorArgs()
    {
        // Arrange
        string expectedName = "foo";
        string expectedPropName = "Bar";

        // Act
        ColumnMetadata columnInfo = new(
            1,
            expectedName,
            expectedPropName,
            new Borm.Reflection.NullableType(typeof(int), isNullable: false),
            Constraints.None
        );

        // Assert
        Assert.Equal(expectedName, columnInfo.Name);
        Assert.Equal(expectedPropName, columnInfo.PropertyName);
    }
}
