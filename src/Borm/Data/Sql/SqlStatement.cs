using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Borm.Properties;

namespace Borm.Data.Sql;

public sealed class SqlStatement
{
    public const char DefaultParameterPrefix = '$';
    private readonly List<object?[]> _batchValues;
    private readonly DbParameter[] _parameters;
    private readonly string _sql;

    public SqlStatement(string sql, DbParameter[] parameters)
    {
        _sql = sql;
        _parameters = parameters;
        _batchValues = [];
    }

    public int BatchValuesCount => _batchValues.Count;
    public DbParameter[] Parameters => _parameters;
    public string Sql => _sql;

    public void PrepareCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = _sql;
        IDataParameterCollection cmdParameters = dbCommand.Parameters;
        cmdParameters.Clear();
        for (int i = 0; i < _parameters.Length; i++)
        {
            cmdParameters.Add(_parameters[i]);
        }
        dbCommand.Prepare();
    }

    public void SetDbParameters(DbCommand dbCommand, int idx)
    {
        object?[] values = _batchValues[idx];
        for (int i = 0; i < values.Length; i++)
        {
            dbCommand.Parameters[i].Value = values[i];
        }
    }

    internal void AddBatchValues(DataRow row)
    {
        DataTable table = row.Table;
        DataColumnCollection columns = table.Columns;
        object?[] values = new object[_parameters.Length];
        for (int i = 0; i < _parameters.Length; i++)
        {
            string columnName = GetColumnNameFromParameterName(_parameters[i].ParameterName);
            if (columns[columnName] == null)
            {
                throw new InvalidOperationException(
                    Strings.SqlStatementParameterRowMapping(columnName, table.TableName)
                );
            }
            values[i] = row[columnName];
        }
        _batchValues.Add(values);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetColumnNameFromParameterName(string parameterName)
    {
        return parameterName[0] == DefaultParameterPrefix ? parameterName[1..] : parameterName;
    }
}
