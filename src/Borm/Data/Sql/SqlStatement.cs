using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Borm.Data.Sql;

public sealed class SqlStatement
{
    public const char DefaultParameterPrefix = '$';
    private readonly DbParameter[] _parameters;
    private readonly string _sql;

    public SqlStatement(string sql, DbParameter[] parameters)
    {
        _sql = sql;
        _parameters = parameters;
    }

    public DbParameter[] Parameters => _parameters;
    public string Sql => _sql;

    internal void PrepareCommand(IDbCommand dbCommand)
    {
        dbCommand.CommandText = _sql;
        dbCommand.Parameters.Clear();
        DbParameter[] parameters = _parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            dbCommand.Parameters.Add(parameters[i]);
        }
        dbCommand.Prepare();
    }

    internal void SetParameters(DataRow row)
    {
        DataColumnCollection columns = row.Table.Columns;
        for (int i = 0; i < Parameters.Length; i++)
        {
            DbParameter current = Parameters[i];
            string columnName = GetColumnNameFromParameterName(current.ParameterName);
            if (columns[columnName] == null)
            {
                throw new InvalidOperationException(
                    "Unable to match column schema of a row and parameter schema"
                );
            }
            current.Value = row[columnName];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetColumnNameFromParameterName(string parameterName)
    {
        return parameterName[0] == DefaultParameterPrefix ? parameterName[1..] : parameterName;
    }
}
