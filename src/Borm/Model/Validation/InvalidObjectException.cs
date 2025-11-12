using System.Diagnostics;
using Borm.Properties;

namespace Borm.Model.Validation;

public sealed class InvalidObjectException : InvalidOperationException
{
    public InvalidObjectException(string? message)
        : base(message)
    {
    }

    public InvalidObjectException(ValidationResult result)
        : base(BuildMessage(result))
    {
    }

    private static string BuildMessage(ValidationResult result)
    {
        Debug.Assert(
            result.IsError,
            "Attempting to build an exception for an OK validation result."
        );

        string message = string.IsNullOrWhiteSpace(result.Message)
            ? Strings.EntityValidationFailed(result.MemberName)
            : Strings.EntityValidationFailedWithUserMessage(result.MemberName, result.Message);

        return message;
    }
}