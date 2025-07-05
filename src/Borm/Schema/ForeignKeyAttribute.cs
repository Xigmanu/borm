namespace Borm.Schema;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ForeignKeyAttribute : ColumnAttribute
{
    private readonly Type _referencedEntityType;

    public ForeignKeyAttribute(int index, string name, Type referencedEntityType)
        : base(index, name)
    {
        _referencedEntityType = referencedEntityType;
    }

    public ForeignKeyAttribute(int index, Type referencedEntityType)
        : base(index)
    {
        _referencedEntityType = referencedEntityType;
    }

    public Type ReferencedEntityType => _referencedEntityType;
}
