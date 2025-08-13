using System.Text;
using Borm.Model;
using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

public sealed class SqliteStatementFactory : ISqlStatementFactory
{
    private const string CreateTableStatementFormat = "CREATE TABLE {0}({1});";
    private const string DeleteStatementFormat = "DELETE FROM {0} WHERE {1};";
    private const string InsertStatementFormat = "INSERT INTO {0} VALUES({1});";
    private const string SelectAllStatementFormat = "SELECT * FROM {0};";
    private const string UpdateStatementFormat = "UPDATE {0} SET {1} WHERE {2};";

    public SqlStatement NewCreateTableStatement(ITable table)
    {
        string tableName = table.Name;
        IEnumerable<IColumn> columns = table.Columns;

        List<string> columnDefinitions = new(columns.Count());
        foreach (IColumn column in columns)
        {
            StringBuilder columnDefinitionBuilder = new StringBuilder().AppendFormat(
                "{0} ",
                column.Name
            );
            string sqliteType = SqliteTypeHelper
                .ToSqliteType(column.DataType)
                .ToString()
                .ToUpperInvariant();
            columnDefinitionBuilder.AppendFormat("{0} ", sqliteType);

            AppendConstraints(table, column, columnDefinitionBuilder);

            columnDefinitions.Add(columnDefinitionBuilder.ToString());
        }
        string columnDefinitionsStr = new StringBuilder()
            .AppendJoin(',', columnDefinitions)
            .ToString();

        string sql = string.Format(CreateTableStatementFormat, tableName, columnDefinitionsStr);
        return new SqlStatement(sql, []);
    }

    public SqlStatement NewDeleteStatement(ITable table)
    {
        IColumn primaryKey = table.PrimaryKey;
        (string expression, SqliteParameter[] parameters) = CreateParametrizedExpression(
            [primaryKey],
            (columnName, paramName) => $"{columnName} = {paramName}"
        );
        string sql = string.Format(DeleteStatementFormat, table.Name, expression);
        return new SqlStatement(sql, parameters);
    }

    public SqlStatement NewInsertStatement(ITable table)
    {
        (string expression, SqliteParameter[] parameters) = CreateParametrizedExpression(
            [.. table.Columns],
            (_, paramName) => paramName
        );
        string sql = string.Format(InsertStatementFormat, table.Name, expression);
        return new SqlStatement(sql, parameters);
    }

    public SqlStatement NewSelectAllStatement(ITable table)
    {
        string sql = string.Format(SelectAllStatementFormat, table.Name);
        return new SqlStatement(sql, []);
    }

    public SqlStatement NewUpdateStatement(ITable table)
    {
        IColumn primaryKey = table.PrimaryKey;
        IColumn[] columns = [.. table.Columns.Where(column => !column.Equals(primaryKey))];

        (string expression, SqliteParameter[] expressionParams) = CreateParametrizedExpression(
            columns,
            (columnName, paramName) => $"{columnName} = {paramName}"
        );

        SqliteParameter conditionalParam = CreateParameterForColumn(primaryKey);
        SqliteParameter[] parameters = new SqliteParameter[expressionParams.Length + 1];
        Array.Copy(expressionParams, parameters, expressionParams.Length);
        parameters[^1] = conditionalParam;

        string sql = string.Format(
            UpdateStatementFormat,
            table.Name,
            expression,
            $"{primaryKey.Name} = {conditionalParam.ParameterName}"
        );
        return new SqlStatement(sql, parameters);
    }

    private static void AppendConstraints(
        ITable table,
        IColumn column,
        StringBuilder columnDefinitionBuilder
    )
    {
        if (column.Constraints.HasFlag(Constraints.PrimaryKey))
        {
            columnDefinitionBuilder.Append("PRIMARY KEY");
            return;
        }

        List<string> constraints = [];
        if (column.Constraints.HasFlag(Constraints.Unique))
        {
            constraints.Add("UNIQUE");
        }

        constraints.Add(column.Constraints.HasFlag(Constraints.AllowDbNull) ? "NULL" : "NOT NULL");
        if (table.ForeignKeyRelations.TryGetValue(column, out ITable? parentTable))
        {
            constraints.Add($"REFERENCES {parentTable.Name}({parentTable.PrimaryKey.Name})");
        }

        columnDefinitionBuilder.AppendJoin(' ', constraints);
    }

    private static SqliteParameter CreateParameterForColumn(IColumn column)
    {
        string paramName = string.Format("${0}", column.Name);
        SqliteType type = SqliteTypeHelper.ToSqliteType(column.DataType);
        return new SqliteParameter(paramName, type);
    }

    private static (string, SqliteParameter[]) CreateParametrizedExpression(
        IColumn[] columns,
        Func<string, string, string> formatter
    )
    {
        SqliteParameter[] parameters = new SqliteParameter[columns.Length];
        string[] expressions = new string[columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            IColumn column = columns[i];

            SqliteParameter parameter = CreateParameterForColumn(column);
            expressions[i] = formatter(column.Name, parameter.ParameterName);
            parameters[i] = parameter;
        }

        string expressionsJoined = new StringBuilder().AppendJoin(',', expressions).ToString();
        return (expressionsJoined, parameters);
    }
}
