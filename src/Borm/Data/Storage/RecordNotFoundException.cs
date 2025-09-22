namespace Borm.Data.Storage;

/// <summary>
/// The exception that is thrown when a table record cannot be found
/// for a given primary key.
/// </summary>
public sealed class RecordNotFoundException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordNotFoundException"/> class.
    /// </summary>
    public RecordNotFoundException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordNotFoundException"/> class
    /// with a specified error message.
    /// </summary>
    public RecordNotFoundException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordNotFoundException"/> class
    /// with a specified error message and a reference to the exception that
    /// is the cause of this exception.
    /// </summary>
    public RecordNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the<see cref = "RecordNotFoundException" /> class
    /// with a specified error message, entity name, and primary key value.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="entityName"></param>
    /// <param name="primaryKey"></param>
    public RecordNotFoundException(string message, string entityName, object primaryKey)
        : base(message)
    {
        EntityName = entityName;
        PrimaryKey = primaryKey;
    }

    /// <summary>
    /// Name of the entity whose row could not be found.
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Primary key value that was used to query the missing row.
    /// </summary>
    public object? PrimaryKey { get; }
}
