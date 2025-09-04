namespace Borm.Model.Metadata;

public sealed class NodeNotFoundException : InvalidOperationException
{
    public NodeNotFoundException() { }

    public NodeNotFoundException(string? message)
        : base(message) { }

    public NodeNotFoundException(string message, Type entityType)
        : base(message)
    {
        EntityType = entityType;
    }

    public NodeNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    public Type? EntityType { get; }
}
