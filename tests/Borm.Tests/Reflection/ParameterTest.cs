using Borm.Reflection;
using Borm.Reflection.Internal;

namespace Borm.Tests.Reflection;

public sealed class ParameterTest
{
    [Fact]
    public void Constructor_InitializesNewInstanceAndSetsProperties()
    {
        // Arrange
        string memberName = "name";
        NullableType dataType = new(typeof(int), false);

        // Act
        Parameter parameter = new(memberName, dataType);

        // Assert
        Assert.Equal(memberName, parameter.MemberName);
        Assert.Equal(dataType, parameter.DataType);
        Assert.Null(parameter.Mapping);
        Assert.Null(parameter.ValidationInfo);
    }
}