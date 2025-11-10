using System.Data;
using System.Data.Common;

namespace Borm.Data.Sql;

/// <summary>
///     Represents the definition of a database command.
/// </summary>
public sealed class DbCommandDefinition
{
    /// <summary>
    ///     The default character prefix used for SQL parameters.
    /// </summary>
    public const char DefaultParameterPrefix = '$';

    internal static readonly DbCommandDefinition Empty = new("sql", []);

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbCommandDefinition" /> class.
    /// </summary>
    public DbCommandDefinition(string sql, DbParameter[] parameters)
    {
        Sql = sql;
        Parameters = parameters;
        BatchQueue = new ParameterBatchQueue();
    }

    public ParameterBatchQueue BatchQueue { get; }
    public DbParameter[] Parameters { get; }

    public string Sql { get; }

    /// <summary>
    ///     Prepares the specified <see cref="IDbCommand" />.
    /// </summary>
    /// <param name="dbCommand">The database command to prepare.</param>
    public void Prepare(IDbCommand dbCommand)
    {
        dbCommand.CommandText = Sql;
        IDataParameterCollection cmdParameters = dbCommand.Parameters;
        cmdParameters.Clear();
        foreach (DbParameter param in Parameters)
        {
            cmdParameters.Add(param);
        }

        dbCommand.Prepare();
    }
}