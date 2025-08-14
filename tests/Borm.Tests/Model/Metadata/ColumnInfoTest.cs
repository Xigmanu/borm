using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;
public sealed class ColumnInfoTest
{
    [Fact]
    public void Constructor_ShouldAssignPropertyNameAndColumnName_WithValidCtorArgs()
    {
        // Arrange
        string expectedName = "foo";
        string expectedPropName = "Bar";

        // Act
        Column columnInfo = new(1, expectedName, expectedPropName, typeof(int), Constraints.None, null);

        // Assert
        Assert.Equal(expectedName, columnInfo.Name);
        Assert.Equal(expectedPropName, columnInfo.PropertyName);
    }
}
