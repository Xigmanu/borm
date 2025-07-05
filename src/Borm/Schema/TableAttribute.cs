namespace Borm.Schema;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class TableAttribute : Attribute
{
    private readonly string? _name;

    public TableAttribute() { }

    public TableAttribute(string name)
    {
        _name = name;
    }

    public string? Name => _name;
}
