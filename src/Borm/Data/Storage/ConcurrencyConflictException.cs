namespace Borm.Data.Storage;

/// <summary>
/// The exception that is thrown when a transaction attempts to modify a record
/// whose state has changed since it was last read.
/// </summary>
public sealed class ConcurrencyConflictException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyConflictException"/> class.
    /// </summary>
    public ConcurrencyConflictException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyConflictException"/> class
    /// with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConcurrencyConflictException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyConflictException"/> class
    /// with the specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public ConcurrencyConflictException(string message, Exception? innerException)
        : base(message, innerException) { }
}
