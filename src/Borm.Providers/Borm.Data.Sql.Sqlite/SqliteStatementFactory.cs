using System.Data;
using System.Diagnostics;
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

    public SqlStatement NewCreateTableStatement(DataTable dataTable)
    {
        string tableName = dataTable.TableName;
        DataColumnCollection columns = dataTable.Columns;
        Dictionary<DataColumn, DataTable> relationMap = GetRelationMap(dataTable);

        string[] columnDefinitions = new string[columns.Count];
        for (int i = 0; i < columns.Count; i++)
        {
            DataColumn column = columns[i];

            StringBuilder columnDefinitionBuilder = new StringBuilder().AppendFormat(
                "{0} ",
                column.ColumnName
            );
            string sqliteType = SqliteTypeHelper
                .ToSqliteType(column.DataType)
                .ToString()
                .ToUpperInvariant();
            columnDefinitionBuilder.AppendFormat("{0} ", sqliteType);
            if (dataTable.PrimaryKey[0].Equals(column))
            {
                columnDefinitionBuilder.Append("PRIMARY KEY");
            }
            else
            {
                columnDefinitionBuilder.Append(column.AllowDBNull ? "NULL" : "NOT NULL");
                if (relationMap.TryGetValue(column, out DataTable? parentTable))
                {
                    columnDefinitionBuilder.AppendFormat(
                        " REFERENCES {0}({1})",
                        parentTable.TableName,
                        parentTable.PrimaryKey[0].ColumnName
                    );
                }
            }

            columnDefinitions[i] = columnDefinitionBuilder.ToString();
        }
        string columnDefinitionsStr = new StringBuilder()
            .AppendJoin(',', columnDefinitions)
            .ToString();

        string sql = string.Format(CreateTableStatementFormat, tableName, columnDefinitionsStr);
        return new SqlStatement(sql, []);
    }

    public SqlStatement NewDeleteStatement(DataTable dataTable)
    {
        DataColumn primaryKey = dataTable.PrimaryKey[0];
        (string expression, SqliteParameter[] parameters) = CreateParametrizedExpression(
            [primaryKey],
            (columnName, paramName) => $"{columnName} = {paramName}"
        );
        string sql = string.Format(DeleteStatementFormat, dataTable.TableName, expression);
        return new SqlStatement(sql, parameters);
    }

    public SqlStatement NewInsertStatement(DataTable dataTable)
    {
        DataColumn[] columns = ToArray(dataTable.Columns);
        (string expression, SqliteParameter[] parameters) = CreateParametrizedExpression(
            columns,
            (_, paramName) => paramName
        );
        string sql = string.Format(InsertStatementFormat, dataTable.TableName, expression);
        return new SqlStatement(sql, parameters);
    }

    public SqlStatement NewSelectAllStatement(DataTable dataTable)
    {
        string sql = string.Format(SelectAllStatementFormat, dataTable.TableName);
        return new SqlStatement(sql, []);
    }

    public SqlStatement NewUpdateStatement(DataTable dataTable)
    {
        DataColumn primaryKey = dataTable.PrimaryKey[0];
        DataColumn[] columns =
        [
            .. ToArray(dataTable.Columns).Where(column => !column.Equals(primaryKey)),
        ];

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
            dataTable.TableName,
            expression,
            $"{primaryKey.ColumnName} = {conditionalParam.ParameterName}"
        );
        return new SqlStatement(sql, parameters);
    }

    private static SqliteParameter CreateParameterForColumn(DataColumn column)
    {
        string paramName = string.Format("${0}", column.ColumnName);
        SqliteType type = SqliteTypeHelper.ToSqliteType(column.DataType);
        return new SqliteParameter(paramName, type);
    }

    private static (string, SqliteParameter[]) CreateParametrizedExpression(
        DataColumn[] columns,
        Func<string, string, string> formatter
    )
    {
        SqliteParameter[] parameters = new SqliteParameter[columns.Length];
        string[] expressions = new string[columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            DataColumn column = columns[i];

            SqliteParameter parameter = CreateParameterForColumn(column);
            expressions[i] = formatter(column.ColumnName, parameter.ParameterName);
            parameters[i] = parameter;
        }

        string expressionsJoined = new StringBuilder().AppendJoin(',', expressions).ToString();
        return (expressionsJoined, parameters);
    }

    private static Dictionary<DataColumn, DataTable> GetRelationMap(DataTable dataTable)
    {
        Dictionary<DataColumn, DataTable> relationMap = [];
        foreach (DataRelation parentRelation in dataTable.ParentRelations)
        {
            DataColumn[] childColumns = parentRelation.ChildColumns;
            Debug.Assert(childColumns.Length == 1);
            relationMap[childColumns[0]] = parentRelation.ParentTable;
        }
        return relationMap;
    }

    private static DataColumn[] ToArray(DataColumnCollection columnCollection)
    {
        DataColumn[] columns = new DataColumn[columnCollection.Count];
        for (int i = 0; i < columns.Length; i++)
        {
            columns[i] = columnCollection[i];
        }
        return columns;
    }
}
