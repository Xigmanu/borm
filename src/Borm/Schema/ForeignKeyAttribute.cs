namespace Borm.Schema;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ForeignKeyAttribute : ColumnAttribute
{
    public ForeignKeyAttribute(int index, string name, Type referencedEntityType)
        : base(index, name)
    {
        ReferencedEntityType = referencedEntityType;
    }

    public ForeignKeyAttribute(int index, Type referencedEntityType)
        : base(index)
    {
        ReferencedEntityType = referencedEntityType;
    }

    public Type ReferencedEntityType { get; }
}
