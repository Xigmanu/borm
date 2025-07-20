namespace Borm.Model;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class PrimaryKeyAttribute : ColumnAttribute
{
    public PrimaryKeyAttribute(int index, string name)
        : base(index, name) { }

    public PrimaryKeyAttribute(int index)
        : base(index) { }
}
