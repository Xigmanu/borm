using System.Diagnostics;

namespace Borm.Util;

internal static class ColumnDataTypeHelper
{
    [DebuggerStepThrough]
    public static bool IsSupported(Type dataType)
    {
        switch (dataType)
        {
            case not null when dataType == typeof(ushort):
            case not null when dataType == typeof(short):
            case not null when dataType == typeof(ulong):
            case not null when dataType == typeof(long):
            case not null when dataType == typeof(uint):
            case not null when dataType == typeof(int):
            case not null when dataType == typeof(float):
            case not null when dataType == typeof(double):
            case not null when dataType == typeof(decimal):
            case not null when dataType == typeof(char):
            case not null when dataType == typeof(bool):
            case not null when dataType == typeof(string):
            case not null when dataType == typeof(Guid):
            case not null when dataType == typeof(DateTime):
                return true;
            default:
                return false;
        }
    }

    public static object Parse(string value, Type targetType) =>
        targetType switch
        {
            not null when targetType == typeof(Guid) => Guid.Parse(value),
            not null when targetType == typeof(DateTime) => Convert.ToDateTime(value),
            _ => value
        };
}