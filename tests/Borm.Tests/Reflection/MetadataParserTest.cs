using Borm.Model;
using Borm.Reflection;

namespace Borm.Tests.Reflection;

public sealed class MetadataParserTest
{
    [Fact]
    public void Parse_ThrowsMemberAccessException_WhenEntityIsNotDecoratedWithEntityAttribute()
    {
        // Arrange
        Type entityType = typeof(MetadataParserTest);
        MetadataParser parser = new();

        // Act
        Exception exception = Record.Exception(() => _ = parser.Parse(entityType, null));

        // Assert
        Assert.IsType<MemberAccessException>(exception);
    }
}
