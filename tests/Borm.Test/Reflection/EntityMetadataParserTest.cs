using Borm.Reflection;
using static Borm.Tests.Mocks.EntityMetadataParserTestMocks;

namespace Borm.Tests.Reflection;

public sealed class EntityMetadataParserTest
{
    [Fact]
    public void Parse_ReturnsReflectedInfo_WithValidEntity()
    {
        // Arrange
        Type entityType = typeof(ValidEntity);
        int numColumns = 2;
        object[][] expectedPropData =
        [
            ["Id", typeof(int), false],
            ["Name", typeof(string), true],
        ];
        EntityMetadataParser parser = new();

        // Act
        ReflectedEntityInfo reflectedInfo = parser.Parse(entityType);

        // Assert
        Assert.Equal(entityType, reflectedInfo.Type);
        Assert.Equal(numColumns, reflectedInfo.Properties.Count());
        for (int i = 0; i < expectedPropData.Length; i++)
        {
            EntityProperty property = reflectedInfo.Properties.ElementAt(i);

            object[] expected = expectedPropData[i];
            Assert.Equal(expected[0], property.Name);
            Assert.Equal(expected[1], property.Type);
            Assert.Equal(expected[2], property.IsNullable);
        }
    }

    [Fact]
    public void Parse_ThrowsMemberAccessException_WhenEntityIsNotDecoratedWithEntityAttribute()
    {
        // Arrange
        Type entityType = typeof(EntityMetadataParserTest);
        EntityMetadataParser parser = new();

        // Act
        Exception exception = Record.Exception(() => _ = parser.Parse(entityType));

        // Assert
        Assert.IsType<MemberAccessException>(exception);
    }
}
