using System.Data;
using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

public sealed class SqliteCommandExecutor : IDbStatementExecutor
{
    private readonly string _connectionString;

    public SqliteCommandExecutor(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void ExecuteNonQuery(SqlStatement statement)
    {
        using SqliteConnection connection = new(_connectionString);
        using SqliteCommand command = connection.CreateCommand();

        connection.Open();
        using SqliteTransaction transaction = connection.BeginTransaction();
        command.Transaction = transaction;

        try
        {
            statement.PrepareCommand(command);

            if (statement.BatchValuesCount > 0)
            {
                for (int i = 0; i < statement.BatchValuesCount; i++)
                {
                    statement.SetDbParameters(command, i);
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

    public IDataReader ExecuteReader(SqlStatement statement)
    {
        using SqliteConnection connection = new(_connectionString);
        using SqliteTransaction transaction = connection.BeginTransaction();
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;

        SqliteDataReader dataReader;
        connection.Open();
        try
        {
            statement.PrepareCommand(command);
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
