using Borm.Model;

namespace Borm.Tests.Model;

public sealed class ForeignKeyAttributeTest
{
    [Fact]
    public void Constructor_InitializesNewInstanceAndSetsProperties()
    {
        // Arrange
        int index = 0;
        string name = "name";
        Type reference = typeof(int);

        // Act
        ForeignKeyAttribute attribute = new(index, name, reference);

        // Assert
        Assert.Equal(index, attribute.Index);
        Assert.Equal(name, attribute.Name);
        Assert.Equal(reference, attribute.Reference);
        Assert.Equal(ReferentialAction.NoAction, attribute.OnDelete);
    }
}