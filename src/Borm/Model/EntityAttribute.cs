namespace Borm.Model;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EntityAttribute : Attribute
{
    public EntityAttribute() { }

    public EntityAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; }
}
