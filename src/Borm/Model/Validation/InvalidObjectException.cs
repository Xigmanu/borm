using System.Diagnostics;
using System.Text;
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

        StringBuilder messageBuilder = new();
        if (!string.IsNullOrWhiteSpace(result.Column) && !string.IsNullOrWhiteSpace(result.Entity))
        {
            messageBuilder.Append($"Validation failed for column '{result.Column}' of entity '{result.Entity}'. ");
        }
        messageBuilder.Append(result.Message);

        return messageBuilder.ToString();
    }
}