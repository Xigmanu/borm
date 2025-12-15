using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

public sealed class SqliteCommandExecutor : IDbCommandExecutor
{
    private readonly string _connectionString;

    public SqliteCommandExecutor(string connectionString)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        _connectionString = connectionString;
    }

    public void ExecuteBatch(DbCommandDefinition command)
    {
        using SqliteConnection connection = new(_connectionString);
        connection.Open();

        using SqliteTransaction transaction = connection.BeginTransaction();
        using SqliteCommand sqliteCommand = connection.CreateCommand();
        sqliteCommand.Transaction = transaction;

        try
        {
            command.Prepare(sqliteCommand);

            ParameterBatchQueue batchQueue = command.BatchQueue;
            if (batchQueue.Count > 0)
            {
                while (batchQueue.HasNext())
                {
                    batchQueue.SetParameterValues(sqliteCommand);
                    _ = sqliteCommand.ExecuteNonQuery();
                }
            }
            else
            {
                _ = sqliteCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task ExecuteBatchAsync(
        DbCommandDefinition command,
        CancellationToken cancellationToken
    )
    {
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using DbTransaction transaction = await connection.BeginTransactionAsync(
            cancellationToken
        );
        await using DbCommand dbCommand = connection.CreateCommand();
        dbCommand.Transaction = transaction;

        try
        {
            command.Prepare(dbCommand);

            ParameterBatchQueue batchQueue = command.BatchQueue;
            if (batchQueue.Count > 0)
            {
                while (batchQueue.HasNext())
                {
                    batchQueue.SetParameterValues(dbCommand);
                    await dbCommand.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            else
            {
                await dbCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public ResultSet Query(DbCommandDefinition command)
    {
        using SqliteConnection connection = new(_connectionString);
        connection.Open();

        using SqliteCommand sqliteCommand = connection.CreateCommand();
        command.Prepare(sqliteCommand);

        using DbDataReader reader = sqliteCommand.ExecuteReader();
        return ResultSet.FromReader(reader);
    }
}