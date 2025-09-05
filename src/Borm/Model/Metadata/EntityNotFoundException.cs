namespace Borm.Model.Metadata;

/// <summary>
/// The exception that is thrown when a requested entity cannot be found
/// within the <see cref="DataContext"/>.
/// </summary>
public sealed class EntityNotFoundException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    public EntityNotFoundException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class
    /// with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EntityNotFoundException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class
    /// with the specified error message and the entity type that was not found.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="entityType">The type of the entity that was expected but not found.</param>
    public EntityNotFoundException(string message, Type entityType)
        : base(message)
    {
        EntityType = entityType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class
    /// with the specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public EntityNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// The type of the entity that was expected but not found,
    /// </summary>
    public Type? EntityType { get; }
}
