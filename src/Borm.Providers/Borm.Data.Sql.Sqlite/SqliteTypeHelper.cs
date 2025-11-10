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
            case not null when type == typeof(ushort):
            case not null when type == typeof(short):
            case not null when type == typeof(ulong):
            case not null when type == typeof(long):
            case not null when type == typeof(uint):
            case not null when type == typeof(int):
                return SqliteType.Integer;
            case not null when type == typeof(float):
            case not null when type == typeof(double):
            case not null when type == typeof(decimal):
                return SqliteType.Real;
            case not null when type == typeof(char):
            case not null when type == typeof(bool):
            case not null when type == typeof(string):
            case not null when type == typeof(Guid):
            case not null when type == typeof(DateTime):
                return SqliteType.Text;
            default:
                throw new NotSupportedException(Strings.TypeNotSupported(type?.FullName!));
        }
    }
}