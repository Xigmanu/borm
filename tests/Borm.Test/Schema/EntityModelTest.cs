using Borm.Reflection;
using Borm.Schema;

namespace Borm.Tests.Schema;

public sealed class EntityModelTest
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenModelTypeEnumerationIsNull()
    {
        // Act
        Exception exception = Record.Exception(() => _ = new EntityModel(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void GetReflectedInfo_ReturnsParsedEntityTypeInfoFromModelTypes()
    {
        // Arrange
        Type entityType = typeof(EntityA);
        EntityModel model = new([entityType]);

        Property idProp = new("Id", new PrimaryKeyAttribute(0, "id"), false, typeof(int));
        Property nameProp = new("Name", new ColumnAttribute(1, "name"), false, typeof(string));
        Property[] properties = [idProp, nameProp];
        ReflectedTypeInfo expected = new(entityType, new EntityAttribute("entities"), properties);

        // Act
        IEnumerable<ReflectedTypeInfo> reflectedInfos = model.GetReflectedInfo();

        // Assert
        Assert.Single(reflectedInfos);
        ReflectedTypeInfo actual = reflectedInfos.First();

        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.Attribute.Name, actual.Attribute.Name);
        Assert.Equal(properties.Length, actual.Properties.Count());

        for (int i = 0; i < properties.Length; i++)
        {
            Property expectedProp = properties[i];
            Property actualProp = actual.Properties.ElementAt(i);

            Assert.Equal(expectedProp.Name, actualProp.Name);
            Assert.Equal(expectedProp.Attribute.Name, actualProp.Attribute.Name);
            Assert.Equal(expectedProp.IsNullable, actualProp.IsNullable);
            Assert.Equal(expectedProp.Type, actualProp.Type);
        }
    }

    [Fact]
    public void GetReflectedInfo_ThrowsArgumentException_WhenEntityTypeIsAbstract()
    {
        // Arrange
        Type entityType = typeof(EntityB);
        EntityModel model = new([entityType]);

        // Act
        Exception exception = Record.Exception(() => _ = model.GetReflectedInfo());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Entity("entities")]
    public sealed class EntityA(int id, string name)
    {
        [PrimaryKey(0, "id")]
        public int Id { get; } = id;

        [Column(1, "name")]
        public string Name { get; } = name;
    }

    [Entity]
    public abstract class EntityB;
}
