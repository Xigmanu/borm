using System.Data.Common;
using System.Diagnostics;
using Borm.Extensions;
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

    public ResultSet ReadRows(DbCommandDefinition command)
    {
        using SqliteConnection connection = new(_connectionString);
        using SqliteCommand sqliteCommand = connection.CreateCommand();

        connection.Open();
        try
        {
            command.Prepare(sqliteCommand);
            using DbDataReader reader = sqliteCommand.ExecuteReader();
            return reader.ToResultSet();
        }
        finally
        {
            connection.Close();
        }
    }

    public bool TableExists(string tableName)
    {
        string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='{0}'";
        using SqliteConnection connection = new(_connectionString);
        using SqliteCommand sqliteCommand = connection.CreateCommand();
        sqliteCommand.CommandText = string.Format(sql, tableName);

        connection.Open();
        try
        {
            using DbDataReader reader = sqliteCommand.ExecuteReader();
            ResultSet resultSet = reader.ToResultSet();
            Debug.Assert(resultSet.RowCount < 2);

            if (resultSet.MoveNext())
            {
                return (string)resultSet.Current["name"] == tableName;
            }

            return false;
        }
        finally
        {
            connection.Close();
        }
    }
}
