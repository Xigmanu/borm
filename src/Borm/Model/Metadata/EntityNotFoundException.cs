namespace Borm.Model.Metadata;

public sealed class EntityNotFoundException : InvalidOperationException
{
    public EntityNotFoundException() { }

    public EntityNotFoundException(string? message)
        : base(message) { }

    public EntityNotFoundException(string message, Type entityType)
        : base(message)
    {
        EntityType = entityType;
    }

    public EntityNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    public Type? EntityType { get; }
}
