using System.Data.Common;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

public sealed class SqliteCommandDefinitionFactory : IDbCommandDefinitionFactory
{
    private const string DeleteStatementFormat = "DELETE FROM {0} WHERE {1};";
    private const string InsertStatementFormat = "INSERT INTO {0} VALUES({1});";
    private const string SelectAllStatementFormat = "SELECT * FROM {0};";
    private const string UpdateStatementFormat = "UPDATE {0} SET {1} WHERE {2};";

    public DbCommandDefinition Delete(TableInfo tableSchema)
    {
        ColumnInfo primaryKey = tableSchema.PrimaryKey;
        (string expression, DbParameter[] parameters) = CreateParametrizedExpression(
            [primaryKey],
            (columnName, paramName) => $"{columnName} = {paramName}"
        );
        string sql = string.Format(DeleteStatementFormat, tableSchema.Name, expression);
        return new DbCommandDefinition(sql, parameters);
    }

    public DbCommandDefinition Insert(TableInfo tableSchema)
    {
        (string expression, DbParameter[] parameters) = CreateParametrizedExpression(
            [.. tableSchema.Columns],
            (_, paramName) => paramName
        );
        string sql = string.Format(InsertStatementFormat, tableSchema.Name, expression);
        return new DbCommandDefinition(sql, parameters);
    }

    public DbCommandDefinition SelectAll(TableInfo tableSchema)
    {
        string sql = string.Format(SelectAllStatementFormat, tableSchema.Name);
        return new DbCommandDefinition(sql, []);
    }

    public DbCommandDefinition Update(TableInfo tableSchema)
    {
        ColumnInfo primaryKey = tableSchema.PrimaryKey;
        ColumnInfo[] columns = [.. tableSchema.Columns.Where(col => !col.Equals(primaryKey))];

        (string expression, SqliteParameter[] expressionParams) = CreateParametrizedExpression(
            columns,
            (columnName, paramName) => $"{columnName} = {paramName}"
        );

        SqliteParameter conditionalParam = CreateParameterForColumn(primaryKey);
        DbParameter[] parameters = new DbParameter[expressionParams.Length + 1];
        Array.Copy(expressionParams, parameters, expressionParams.Length);
        parameters[^1] = conditionalParam;

        string sql = string.Format(
            UpdateStatementFormat,
            tableSchema.Name,
            expression,
            $"{primaryKey.Name} = {conditionalParam.ParameterName}"
        );
        return new DbCommandDefinition(sql, parameters);
    }

    private static SqliteParameter CreateParameterForColumn(ColumnInfo column)
    {
        string paramName = $"${column.Name}";
        SqliteType type = SqliteTypeHelper.ToSqliteType(column.DataType);
        return new SqliteParameter(paramName, type);
    }

    private static (string, SqliteParameter[]) CreateParametrizedExpression(
        ColumnInfo[] columns,
        Func<string, string, string> formatter
    )
    {
        SqliteParameter[] parameters = new SqliteParameter[columns.Length];
        string[] expressions = new string[columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            ColumnInfo column = columns[i];

            SqliteParameter parameter = CreateParameterForColumn(column);
            expressions[i] = formatter(column.Name, parameter.ParameterName);
            parameters[i] = parameter;
        }

        string expressionsJoined = new StringBuilder().AppendJoin(',', expressions).ToString();
        return (expressionsJoined, parameters);
    }
}