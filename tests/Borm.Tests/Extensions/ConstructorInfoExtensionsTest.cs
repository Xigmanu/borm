using System.Reflection;
using Borm.Extensions;

namespace Borm.Tests.Extensions;

public sealed class ConstructorInfoExtensionsTest
{
    [Fact]
    public void IsNoArgs_ReturnsFalse_IfConstructorHasArguments()
    {
        // Arrange
        ConstructorInfo ctor = typeof(TestType).GetConstructor([typeof(int)])!;

        // Act
        bool isNoArgs = ConstructorInfoExtensions.IsNoArgs(ctor);

        // Assert
        Assert.False(isNoArgs);
    }

    [Fact]
    public void IsNoArgs_ReturnsTrue_IfConstructorIsNoArgs()
    {
        // Arrange
        ConstructorInfo ctor = typeof(TestType).GetConstructor(Type.EmptyTypes)!;

        // Act
        bool isNoArgs = ConstructorInfoExtensions.IsNoArgs(ctor);

        // Assert
        Assert.True(isNoArgs);
    }

    private sealed class TestType
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public TestType(int value)
        {
            Value = value;
        }
#pragma warning restore S1144 // Unused private types or members should be removed

        public TestType() { }

        public int Value { get; }
    }
}
