using System.Diagnostics;
using System.Reflection;

namespace Borm.Tests.Extensions;

public sealed class MemberInfoExtensionsTest
{
    [Fact]
    public void HasAttribute_ReturnsExpectedResult_BasedOnPresenceOfAttribute()
    {
        // Arrange
        MemberInfo memberInfo = typeof(MemberInfoExtensionsTest);

        // Act
        bool hasAttribute =
            Borm.Extensions.MemberInfoExtensions.HasAttribute<DebuggerStepThroughAttribute>(
                memberInfo
            );

        // Assert
        Assert.False(hasAttribute);
    }

    [Fact]
    public void HasAttribute_ReturnsTrue_IfTypeIsDecoratedWithAttribute()
    {
        // Arrange
        MemberInfo memberInfo = typeof(TypeWithAttribute);

        // Act
        bool hasAttribute =
            Borm.Extensions.MemberInfoExtensions.HasAttribute<DebuggerStepThroughAttribute>(
                memberInfo
            );

        // Assert
        Assert.True(hasAttribute);
    }

    [DebuggerStepThrough]
    private class TypeWithAttribute;
}
