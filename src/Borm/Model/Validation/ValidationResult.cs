using Borm.Properties;

namespace Borm.Model.Validation;

public readonly struct ValidationResult
{
    public static readonly ValidationResult Ok = new(
        string.Empty,
        string.Empty,
        string.Empty,
        false
    );

    private ValidationResult(string message, string entity, string column, bool isError)
    {
        Message = message;
        Entity = entity;
        Column = column;
        IsError = isError;
    }

    public string Column { get; }
    public string Entity { get; }
    public string Message { get; }
    internal bool IsError { get; }

    public static ValidationResult Error(string message) =>
        new(message, string.Empty, string.Empty, true);

    internal static ValidationResult Error(object? value, string entity, string column) =>
        new(Strings.ColumnValueInvalid(value), entity, column, true);
}