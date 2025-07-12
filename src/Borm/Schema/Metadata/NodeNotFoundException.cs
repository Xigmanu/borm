namespace Borm.Schema.Metadata;

public sealed class NodeNotFoundException : InvalidOperationException
{
    public NodeNotFoundException(string message, Type entityType)
        : base(message)
    {
        EntityType = entityType;
    }

    public Type EntityType { get; }
}
