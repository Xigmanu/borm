namespace Borm.Model.Validation;

public sealed class InvalidObjectException : InvalidOperationException
{
    public InvalidObjectException()
    {
    }

    public InvalidObjectException(string? message) : base(message)
    {
    }
}