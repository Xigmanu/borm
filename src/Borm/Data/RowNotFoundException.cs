using System.Data;

namespace Borm.Data;

public sealed class RowNotFoundException : InvalidOperationException
{
    public RowNotFoundException(string message, Type entityType, object primaryKey)
        : base(message)
    {
        EntityType = entityType;
        PrimaryKey = primaryKey;
    }

    public Type EntityType { get; }
    public object PrimaryKey { get; }
}
