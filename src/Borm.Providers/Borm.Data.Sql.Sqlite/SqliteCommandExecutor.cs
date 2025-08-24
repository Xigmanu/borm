using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

public sealed class SqliteCommandExecutor : IDbCommandExecutor
{
    private readonly string _connectionString;

    public SqliteCommandExecutor(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void ExecuteBatch(DbCommandDefinition command)
    {
        using SqliteConnection connection = new(_connectionString);
        using SqliteCommand sqliteCommand = connection.CreateCommand();

        connection.Open();
        using SqliteTransaction transaction = connection.BeginTransaction();
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
            connection.Close();
            throw;
        }
        connection.Close();
    }

    public async Task ExecuteBatchAsync(DbCommandDefinition command)
    {
        await using SqliteConnection connection = new(_connectionString);
        await using DbCommand dbCommand = connection.CreateCommand();

        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
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
                    await dbCommand.ExecuteNonQueryAsync();
                }
            }
            else
            {
                await dbCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            await connection.CloseAsync();
            throw;
        }

        await connection.CloseAsync();
    }

    public DbDataReader ExecuteReader(DbCommandDefinition command)
    {
        using SqliteConnection connection = new(_connectionString);
        using SqliteTransaction transaction = connection.BeginTransaction();
        using SqliteCommand sqliteCommand = connection.CreateCommand();
        sqliteCommand.Transaction = transaction;

        SqliteDataReader dataReader;
        connection.Open();
        try
        {
            command.Prepare(sqliteCommand);
            dataReader = sqliteCommand.ExecuteReader();
        }
        catch
        {
            transaction.Rollback();
            connection.Close();
            throw;
        }

        return dataReader;
    }
}
