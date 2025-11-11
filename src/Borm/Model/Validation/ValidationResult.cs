using System.Diagnostics;
using System.Runtime.CompilerServices;
using Borm.Properties;

namespace Borm.Model.Validation;

public readonly struct ValidationResult
{
    public static readonly ValidationResult Ok = new(string.Empty, string.Empty, false);
    private readonly string _memberName;
    private readonly string? _message;

    private ValidationResult(string? message, string memberName, bool isError)
    {
        _message = message;
        _memberName = memberName;
        IsError = isError;
    }

    internal bool IsError { get; }

    public static ValidationResult Error(object? value, string? message = null,
        [CallerArgumentExpression(nameof(value))]
        string? expression = null)
    {
        Debug.Assert(value != null || value == null);
        if (string.IsNullOrEmpty(expression))
        {
            throw new ArgumentException(Strings.MissingArgumentExpressionString(), nameof(expression));
        }

        string[] split = expression.Split('.');
        Debug.Assert(split.Length > 1);

        return new ValidationResult(message, split[^1], true);
    }

    internal InvalidOperationException BuildException()
    {
        Debug.Assert(IsError, "Attempting to build an exception for an OK validation result.");

        string message = string.IsNullOrWhiteSpace(_message)
            ? Strings.EntityValidationFailed(_memberName)
            : Strings.EntityValidationFailedWithUserMessage(_memberName, _message);

        return new InvalidObjectException(message);
    }
}