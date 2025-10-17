using Borm.Model;
using Borm.Reflection;

namespace Borm.Tests.Model;

public sealed class EntityModelTest
{
    [Fact]
    public void AddEntity_AddsNewEntityWithValidator()
    {
        // Arrange
        Type entityType = typeof(EntityA);
        EntityAValidator validator = new();
        EntityModel model = new();

        // Act
        model.AddEntity(entityType, validator);

        // Assert
        Assert.NotNull(model.GetValidatorFunc(entityType));
    }

    [Fact]
    public void AddEntity_ThrowsArgumentException_WhenEntityTypeIsNotDecoratedWithEntityAttribute()
    {
        // Arrange
        EntityModel model = new();

        // Act
        Exception exception = Record.Exception(() => model.AddEntity(typeof(string)));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }
    /*
    [Fact]
    public void GetReflectedInfo_ReturnsParsedEntityTypeInfoFromModelTypes()
    {
        // Arrange
        Type entityType = typeof(EntityA);
        EntityModel model = new();
        model.AddEntity(entityType);

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
    }*/

    [Fact]
    public void GetReflectedInfo_ThrowsArgumentException_WhenEntityTypeIsAbstract()
    {
        // Arrange
        EntityModel model = new();
        model.AddEntity(typeof(EntityB));

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

    public sealed class EntityAValidator : IEntityValidator<EntityA>
    {
        public void Validate(EntityA entity) { }
    }

    [Entity]
    public abstract class EntityB;
}
