using System.Diagnostics;

namespace Borm.Data.Storage;

internal static class TypeParser
{
    // TODO Support more types
    // TODO Also It would probably make sense to create a general list of supported target types (column data types)
    public static object Parse(object value, Type targetType)
    {
        return targetType switch
        {
            Type _ when targetType == typeof(Guid) => ParseGuid(value),
            Type _ when targetType == typeof(DateTime) => Convert.ToDateTime(value),
            _ => value,
        };
    }

    private static Guid ParseGuid(object value)
    {
        Debug.Assert(value is string || value is byte[]);
        if (value is string str)
        {
            return Guid.Parse(str);
        }
        else if (value is byte[] bytes)
        {
            return new Guid(bytes);
        }

        throw new NotSupportedException();
    }
}
