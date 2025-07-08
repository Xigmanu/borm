namespace Borm.Schema;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    public ColumnAttribute(int index)
    {
        Index =
            index < 0
                ? throw new ArgumentException("Column index cannot be lesser than zero")
                : index;
    }

    public ColumnAttribute(int index, string name)
        : this(index)
    {
        Name = name;
    }

    public int Index { get; }

    public string? Name { get; }
}
