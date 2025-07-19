using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Borm.Properties;

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

    public void PrepareCommand(IDbCommand dbCommand)
    {
        dbCommand.CommandText = _sql;
        IDataParameterCollection cmdParameters = dbCommand.Parameters;
        for (int i = 0; i < _parameters.Length; i++)
        {
            if (cmdParameters.Contains(_parameters[i].ParameterName))
            {
                cmdParameters[i] = _parameters[i].Value;
            }
            else
            {
                cmdParameters.Add(_parameters[i]);
            }
        }
        dbCommand.Prepare();
    }

    internal void SetParameters(DataRow row)
    {
        DataTable table = row.Table;
        DataColumnCollection columns = table.Columns;
        for (int i = 0; i < Parameters.Length; i++)
        {
            DbParameter current = Parameters[i];
            string columnName = GetColumnNameFromParameterName(current.ParameterName);
            if (columns[columnName] == null)
            {
                throw new InvalidOperationException(
                    Strings.SqlStatementParameterRowMapping(columnName, table.TableName)
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
