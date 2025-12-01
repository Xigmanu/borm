using System.Reflection;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Reflection;

public sealed class ConstructorParserTest
{
    [Fact]
    public void ParseAll_ReturnsParsedConstructorInfo_WithValidConstructor()
    {
        // Arrange
        ConstructorInfo constructor = typeof(AddressEntity).GetConstructors()[0];

        // Act
        IConstructor parsed = ConstructorParser.Parse(constructor);

        // Assert
        ParameterInfo[] parameters = constructor.GetParameters();
        Assert.Equal(parameters.Length, parsed.Parameters.Count);
        for (int i = 0; i < parsed.Parameters.Count; i++)
        {
            ParameterInfo expected = parameters[i];
            IMappable actual = parsed.Parameters[i];
            
            Assert.Equal(expected.Name, actual.MemberName);
            Assert.Equal(expected.ParameterType, actual.DataType.RawType);
            Assert.Null(actual.Mapping);
        }
        Assert.False(parsed.IsDefault);
    }
}
