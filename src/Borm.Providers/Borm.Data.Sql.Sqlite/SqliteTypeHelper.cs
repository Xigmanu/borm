using System.Diagnostics;
using Borm.Properties;
using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

internal static class SqliteTypeHelper
{
    [DebuggerStepThrough]
    public static SqliteType ToSqliteType(Type type)
    {
        switch (type)
        {
            case Type _ when type == typeof(ushort):
            case Type _ when type == typeof(short):
            case Type _ when type == typeof(ulong):
            case Type _ when type == typeof(long):
            case Type _ when type == typeof(uint):
            case Type _ when type == typeof(int):
                return SqliteType.Integer;
            case Type _ when type == typeof(float):
            case Type _ when type == typeof(double):
            case Type _ when type == typeof(decimal):
                return SqliteType.Real;
            case Type _ when type == typeof(char):
            case Type _ when type == typeof(bool):
            case Type _ when type == typeof(string):
            case Type _ when type == typeof(Guid):
            case Type _ when type == typeof(DateTime):
                return SqliteType.Text;
            default:
                throw new NotSupportedException(Strings.TypeNotSupported(type.FullName!));
        }
    }
}
