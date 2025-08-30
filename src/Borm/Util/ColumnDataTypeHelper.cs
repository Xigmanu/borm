using System.Diagnostics;

namespace Borm.Util;

[DebuggerStepThrough]
internal static class ColumnDataTypeHelper
{
    public static bool IsSupported(Type dataType)
    {
        switch (dataType)
        {
            case Type _ when dataType == typeof(ushort):
            case Type _ when dataType == typeof(short):
            case Type _ when dataType == typeof(ulong):
            case Type _ when dataType == typeof(long):
            case Type _ when dataType == typeof(uint):
            case Type _ when dataType == typeof(int):
            case Type _ when dataType == typeof(float):
            case Type _ when dataType == typeof(double):
            case Type _ when dataType == typeof(decimal):
            case Type _ when dataType == typeof(char):
            case Type _ when dataType == typeof(bool):
            case Type _ when dataType == typeof(string):
            case Type _ when dataType == typeof(Guid):
            case Type _ when dataType == typeof(DateTime):
                return true;
            default:
                return false;
        }
    }

    public static object Parse(string value, Type targetType)
    {
        return targetType switch
        {
            Type _ when targetType == typeof(Guid) => Guid.Parse(value),
            Type _ when targetType == typeof(DateTime) => Convert.ToDateTime(value),
            _ => value,
        };
    }
}
