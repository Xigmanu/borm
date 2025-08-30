using Borm.Util;

namespace Borm.Tests.Util;

public sealed class ColumnDataTypeHelperTest
{
    public static readonly IEnumerable<object[]> SupportedTypes =
    [
        [typeof(ushort)],
        [typeof(short)],
        [typeof(ulong)],
        [typeof(long)],
        [typeof(uint)],
        [typeof(int)],
        [typeof(float)],
        [typeof(double)],
        [typeof(decimal)],
        [typeof(char)],
        [typeof(bool)],
        [typeof(string)],
        [typeof(Guid)],
        [typeof(DateTime)],
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
}
