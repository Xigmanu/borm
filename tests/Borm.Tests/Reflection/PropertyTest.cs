using Borm.Model;
using Borm.Reflection;
using Borm.Reflection.Internal;

namespace Borm.Tests.Reflection;

public sealed class PropertyTest
{
    [Fact]
    public void Constructor_InitializesNewInstanceAndSetsProperties()
    {
        // Arrange
        string memberName = "name";
        NullableType dataType = new(typeof(int), false);
        MappingInfo mapping = new(0, null, false, false, null, ReferentialAction.NoAction);
        ValidationInfo? validation = null;

        // Act
        Property property = new(memberName, dataType, mapping, validation);

        // Assert
        Assert.Equal(memberName, property.MemberName);
        Assert.Equal(dataType, property.DataType);
        Assert.Equal(mapping, property.Mapping);
        Assert.Equal(validation, property.ValidationInfo);
    }
}