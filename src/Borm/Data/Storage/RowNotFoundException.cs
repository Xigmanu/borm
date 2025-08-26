namespace Borm.Data.Storage;

public sealed class RowNotFoundException : InvalidOperationException
{
    public RowNotFoundException(string message, string entityName, object primaryKey)
        : base(message)
    {
        EntityName = entityName;
        PrimaryKey = primaryKey;
    }

    public string EntityName { get; }
    public object PrimaryKey { get; }
}
