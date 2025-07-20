using System.Data;
using System.Data.Common;

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
        BatchQueue = new();
    }

    public ParameterBatchQueue BatchQueue { get; }
    public DbParameter[] Parameters => _parameters;
    public string Sql => _sql;

    public void Prepare(IDbCommand dbCommand)
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
}
