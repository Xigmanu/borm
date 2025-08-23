namespace Borm.Data.Sql;

public interface ISqlStatementFactory
{
    SqlStatement NewCreateTableStatement(TableInfo tableSchema);
    SqlStatement NewDeleteStatement(TableInfo tableSchema);

    SqlStatement NewInsertStatement(TableInfo tableSchema);

    SqlStatement NewSelectAllStatement(TableInfo tableSchema);
    SqlStatement NewUpdateStatement(TableInfo tableSchema);
}
