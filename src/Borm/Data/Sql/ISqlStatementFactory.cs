namespace Borm.Data.Sql;

public interface ISqlStatementFactory
{
    SqlStatement NewCreateTableStatement(ITable table);
    SqlStatement NewDeleteStatement(ITable table);

    SqlStatement NewInsertStatement(ITable table);

    SqlStatement NewSelectAllStatement(ITable table);
    SqlStatement NewUpdateStatement(ITable table);
}
