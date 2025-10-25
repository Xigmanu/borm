using Borm.Model;
using Borm.Reflection;

namespace Borm.Tests.Model;

public sealed class EntityModelTest
{
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
