using System.Data;
using System.Data.Common;

namespace Borm.Data.Sql;

/// <summary>
/// Represents the definition of a database command.
/// </summary>
public sealed class DbCommandDefinition
{
    /// <summary>
    /// The default character prefix used for SQL parameters.
    /// </summary>
    public const char DefaultParameterPrefix = '$';
    internal static readonly DbCommandDefinition Empty = new("sql", []);

    private readonly DbParameter[] _parameters;
    private readonly string _sql;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbCommandDefinition"/> class.
    /// </summary>
    public DbCommandDefinition(string sql, DbParameter[] parameters)
    {
        _sql = sql;
        _parameters = parameters;
        BatchQueue = new();
    }

    public ParameterBatchQueue BatchQueue { get; }
    public DbParameter[] Parameters => _parameters;

    public string Sql => _sql;

    /// <summary>
    /// Prepares the specified <see cref="IDbCommand"/>.
    /// </summary>
    /// <param name="dbCommand">The database command to prepare.</param>
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
