using System.Diagnostics;
using System.Runtime.CompilerServices;
using Borm.Properties;

namespace Borm.Model.Validation;

public readonly struct ValidationResult
{
    public static readonly ValidationResult Ok = new(null, string.Empty, false);

    private ValidationResult(string? message, string memberName, bool isError)
    {
        Message = message;
        MemberName = memberName;
        IsError = isError;
    }

    public string MemberName { get; }
    public string? Message { get; }
    internal bool IsError { get; }

    public static ValidationResult Error(
        object? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))]
        string? expression = null
    )
    {
        Debug.Assert(value != null || value == null);
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException(
                Strings.MissingArgumentExpressionString(),
                nameof(expression)
            );
        }

        string[] split = expression.Split('.');
        Debug.Assert(split.Length > 1);

        return new ValidationResult(message, split[^1], true);
    }
}