using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

public sealed class SqliteCommandExecutor : IDbStatementExecutor
{
    private readonly string _connectionString;

    public SqliteCommandExecutor(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void ExecuteBatch(SqlStatement statement)
    {
        using SqliteConnection connection = new(_connectionString);
        using SqliteCommand command = connection.CreateCommand();

        connection.Open();
        using SqliteTransaction transaction = connection.BeginTransaction();
        command.Transaction = transaction;

        try
        {
            statement.Prepare(command);

            ParameterBatchQueue batchQueue = statement.BatchQueue;
            if (batchQueue.Count > 0)
            {
                while (batchQueue.Next())
                {
                    batchQueue.SetParameterValues(command);
                    _ = command.ExecuteNonQuery();
                }
            }
            else
            {
                _ = command.ExecuteNonQuery();
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

    public async Task ExecuteBatchAsync(SqlStatement statement)
    {
        await using SqliteConnection connection = new(_connectionString);
        await using DbCommand command = connection.CreateCommand();

        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction;

        try
        {
            statement.Prepare(command);

            ParameterBatchQueue batchQueue = statement.BatchQueue;
            if (batchQueue.Count > 0)
            {
                while (batchQueue.Next())
                {
                    batchQueue.SetParameterValues(command);
                    await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                await command.ExecuteNonQueryAsync();
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

    public DbDataReader ExecuteReader(SqlStatement statement)
    {
        using SqliteConnection connection = new(_connectionString);
        using SqliteTransaction transaction = connection.BeginTransaction();
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;

        SqliteDataReader dataReader;
        connection.Open();
        try
        {
            statement.Prepare(command);
            dataReader = command.ExecuteReader();
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
