using Borm.Schema;

namespace Borm.Reflection;

internal sealed class Property
{
    public Property(string name, ColumnAttribute attribute, bool isNullable, Type type)
    {
        Name = name;
        Attribute = attribute;
        IsNullable = isNullable;
        Type = type;
    }

    public ColumnAttribute Attribute { get; }
    public bool IsNullable { get; }
    public string Name { get; }
    public Type Type { get; }
}
