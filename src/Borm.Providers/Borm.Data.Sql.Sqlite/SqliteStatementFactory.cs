using System.Text;
using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

public sealed class SqliteStatementFactory : ISqlStatementFactory
{
    private const string CreateTableStatementFormat = "CREATE TABLE {0}({1});";
    private const string DeleteStatementFormat = "DELETE FROM {0} WHERE {1};";
    private const string InsertStatementFormat = "INSERT INTO {0} VALUES({1});";
    private const string SelectAllStatementFormat = "SELECT * FROM {0};";
    private const string UpdateStatementFormat = "UPDATE {0} SET {1} WHERE {2};";

    public SqlStatement NewCreateTableStatement(TableInfo tableSchema)
    {
        string tableName = tableSchema.Name;
        IEnumerable<ColumnInfo> columns = tableSchema.Columns;

        List<string> columnDefinitions = new(columns.Count());
        foreach (ColumnInfo column in columns)
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

            AppendConstraints(tableSchema, column, columnDefinitionBuilder);

            columnDefinitions.Add(columnDefinitionBuilder.ToString());
        }
        string columnDefinitionsStr = new StringBuilder()
            .AppendJoin(',', columnDefinitions)
            .ToString();

        string sql = string.Format(CreateTableStatementFormat, tableName, columnDefinitionsStr);
        return new SqlStatement(sql, []);
    }

    public SqlStatement NewDeleteStatement(TableInfo tableSchema)
    {
        ColumnInfo primaryKey = tableSchema.PrimaryKey;
        (string expression, SqliteParameter[] parameters) = CreateParametrizedExpression(
            [primaryKey],
            (columnName, paramName) => $"{columnName} = {paramName}"
        );
        string sql = string.Format(DeleteStatementFormat, tableSchema.Name, expression);
        return new SqlStatement(sql, parameters);
    }

    public SqlStatement NewInsertStatement(TableInfo tableSchema)
    {
        (string expression, SqliteParameter[] parameters) = CreateParametrizedExpression(
            [.. tableSchema.Columns],
            (_, paramName) => paramName
        );
        string sql = string.Format(InsertStatementFormat, tableSchema.Name, expression);
        return new SqlStatement(sql, parameters);
    }

    public SqlStatement NewSelectAllStatement(TableInfo tableSchema)
    {
        string sql = string.Format(SelectAllStatementFormat, tableSchema.Name);
        return new SqlStatement(sql, []);
    }

    public SqlStatement NewUpdateStatement(TableInfo tableSchema)
    {
        ColumnInfo primaryKey = tableSchema.PrimaryKey;
        ColumnInfo[] columns = [.. tableSchema.Columns];

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
            tableSchema.Name,
            expression,
            $"{primaryKey.Name} = {conditionalParam.ParameterName}"
        );
        return new SqlStatement(sql, parameters);
    }

    private static void AppendConstraints(
        TableInfo tableSchema,
        ColumnInfo column,
        StringBuilder columnDefinitionBuilder
    )
    {
        if (tableSchema.PrimaryKey.Equals(column))
        {
            columnDefinitionBuilder.Append("PRIMARY KEY");
            return;
        }

        List<string> constraints = [];
        if (column.IsUnique)
        {
            constraints.Add("UNIQUE");
        }

        constraints.Add(column.IsNullable ? "NULL" : "NOT NULL");
        if (tableSchema.ForeignKeyRelations.TryGetValue(column, out TableInfo? parentTable))
        {
            constraints.Add($"REFERENCES {parentTable.Name}({parentTable.PrimaryKey.Name})");
        }

        columnDefinitionBuilder.AppendJoin(' ', constraints);
    }

    private static SqliteParameter CreateParameterForColumn(ColumnInfo column)
    {
        string paramName = string.Format("${0}", column.Name);
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
