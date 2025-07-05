namespace Borm.Schema;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    private readonly int _index;
    private readonly string? _name;

    public ColumnAttribute(int index)
    {
        _index =
            index < 0
                ? throw new ArgumentException("Column index cannot be lesser than zero")
                : index;
    }

    public ColumnAttribute(int index, string name)
        : this(index)
    {
        _name = name;
    }

    public int Index => _index;

    public string? Name => _name;
}
