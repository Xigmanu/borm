namespace Borm.Schema;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EntityAttribute : Attribute
{
    private readonly string? _name;

    public EntityAttribute() { }

    public EntityAttribute(string name)
    {
        _name = name;
    }

    public string? Name => _name;
}
