using System.Reflection;
using Borm.Reflection;

namespace Borm.Tests.Reflection;

public sealed class NullableTypeTest
{
    [Fact]
    public void UnderlyingType_ReturnsType_WhenTypeIsReferenceType()
    {
        // Arrange
        Type type = typeof(NullableTypeTest);
        bool isNullable = false;

        NullableType nullableType = new(type, isNullable);

        // Act
        Type actual = nullableType.UnderlyingType;

        // Assert
        Assert.Equal(type, actual);
    }

    [Fact]
    public void UnderlyingType_ReturnsType_WhenTypeIsValueType()
    {
        // Arrange
        Type type = typeof(int);
        bool isNullable = false;

        NullableType nullableType = new(type, isNullable);

        // Act
        Type actual = nullableType.UnderlyingType;

        // Assert
        Assert.Equal(type, actual);
    }

    [Fact]
    public void UnderlyingType_ReturnsUnderlyingType_WhenTypeIsValueTypeAndNullable()
    {
        // Arrange
        Type type = typeof(int?);
        Type expected = typeof(int);
        bool isNullable = true;

        NullableType nullableType = new(type, isNullable);

        // Act
        Type actual = nullableType.UnderlyingType;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WrapMemberType_ShouldDetectNonNullableProperty()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableString))!;

        // Act
        NullableType result = NullableType.WrapMemberType(property);

        // Assert
        Assert.Equal(typeof(string), result.Type);
        Assert.False(result.IsNullable);
    }

    [Fact]
    public void WrapMemberType_ShouldDetectNullableProperty()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.NullableString))!;

        // Act
        NullableType result = NullableType.WrapMemberType(property);

        // Assert
        Assert.Equal(typeof(string), result.Type);
        Assert.True(result.IsNullable);
    }

    [Fact]
    public void WrapMemberType_ShouldDetectValueTypeProperty()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.IntProperty))!;

        // Act
        NullableType result = NullableType.WrapMemberType(property);

        // Assert
        Assert.Equal(typeof(int), result.Type);
        Assert.False(result.IsNullable);
    }

    [Fact]
    public void WrapMemberType_ThrowsNotSupportedException_WhenMemberTypeIsNotSupported()
    {
        // Arrange
        Type type = typeof(object);

        // Act
        Exception? exception = Record.Exception(() => _ = NullableType.WrapMemberType(type));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<NotSupportedException>(exception);
    }

#pragma warning disable S3459
    private sealed class TestClass
    {
        public int IntProperty { get; set; }
        public string NonNullableString { get; set; } = null!;
        public string? NullableString { get; set; }
    }
#pragma warning restore S3459
}
