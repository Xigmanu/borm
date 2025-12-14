using Borm.Util;

namespace Borm.Tests.Util;

public sealed class ColumnDataTypeHelperTest
{
    public static readonly TheoryData<(object, Type)> ParseArgs =
    [
        (Guid.NewGuid(), typeof(Guid)),
        (DateTime.Now.Date, typeof(DateTime))
    ];

    public static readonly TheoryData<Type> SupportedTypes =
    [
        typeof(ushort),
        typeof(short),
        typeof(ulong),
        typeof(long),
        typeof(uint),
        typeof(int),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(char),
        typeof(bool),
        typeof(string),
        typeof(Guid),
        typeof(DateTime)
    ];

    [Fact]
    public void IsSupported_ReturnsFalse_WhenProvidedTypeIsNotSupported()
    {
        // Act
        bool isSupported = ColumnDataTypeHelper.IsSupported(typeof(Math));

        // Assert
        Assert.False(isSupported);
    }

    [Theory]
    [MemberData(nameof(SupportedTypes))]
    public void IsSupported_ReturnsTrue_WhenProvidedTypeIsSupported(Type dataType)
    {
        // Act
        bool isSupported = ColumnDataTypeHelper.IsSupported(dataType);

        // Assert
        Assert.True(isSupported);
    }

    [Fact]
    public void Parse_ReturnsArgValue_WhenTargetTypeNotSupported()
    {
        // Arrange
        Type targetType = typeof(string);
        string value = "foo";

        // Act
        object actual = ColumnDataTypeHelper.Parse(value, targetType);

        // Assert
        Assert.Equal(value, actual);
    }

    [Theory]
    [MemberData(nameof(ParseArgs))]
    public void Parse_ReturnsParsedValue_ForSupportedTypes((object, Type) argPair)
    {
        // Arrange
        (object value, Type targetType) = argPair;
        string valueStr = value.ToString()!;

        // Act
        object actual = ColumnDataTypeHelper.Parse(valueStr, targetType);

        // Assert
        Assert.Equal(value, actual);
    }
}