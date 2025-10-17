using Borm.Model;
using Borm.Reflection;

namespace Borm.Tests.Reflection;

public sealed class MetadataParserTest
{
    /*
    [Fact]
    public void Parse_ReturnsReflectedInfo_WithValidEntity()
    {
        // Arrange
        Type entityType = typeof(EntityMetadataParserTestMocks.ValidEntity);
        int numColumns = 2;
        object[][] expectedPropData =
        [
            ["Id", typeof(int), false],
            ["Name", typeof(string), true],
        ];
        MetadataParser parser = new();

        // Act
        ReflectedTypeInfo reflectedInfo = parser.Parse(entityType);

        // Assert
        Assert.Equal(entityType, reflectedInfo.Type);
        Assert.Equal(numColumns, reflectedInfo.Properties.Count());
        for (int i = 0; i < expectedPropData.Length; i++)
        {
            Property property = reflectedInfo.Properties.ElementAt(i);

            object[] expected = expectedPropData[i];
            Assert.Equal(expected[0], property.Name);
            Assert.Equal(expected[1], property.Type);
            Assert.Equal(expected[2], property.IsNullable);
        }
    }*/

    [Fact]
    public void Parse_ThrowsMemberAccessException_WhenEntityIsNotDecoratedWithEntityAttribute()
    {
        // Arrange
        Type entityType = typeof(MetadataParserTest);
        MetadataParser parser = new();

        // Act
        Exception exception = Record.Exception(() => _ = parser.Parse(entityType));

        // Assert
        Assert.IsType<MemberAccessException>(exception);
    }

    private static class EntityMetadataParserTestMocks
    {
        [Entity("entities")]
        public sealed class ValidEntity
        {
            [PrimaryKey(0)]
            public int Id { get; }

            [Column(1, "entity_name")]
            public string? Name { get; }
#pragma warning disable S1144
            public bool Exists { get; }
#pragma warning restore S1144
        }
    }
}
