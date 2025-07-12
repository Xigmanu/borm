namespace Borm.Schema;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ForeignKeyAttribute : ColumnAttribute
{
    public ForeignKeyAttribute(int index, string name, Type reference)
        : base(index, name)
    {
        Reference = reference;
    }

    public ForeignKeyAttribute(int index, Type reference)
        : base(index)
    {
        Reference = reference;
    }

    public Type Reference { get; }
}
