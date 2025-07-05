using System.Data;

namespace Borm.Data.Sql;

public interface ISqlStatementFactory
{
    SqlStatement NewCreateTableStatement(DataTable dataTable);
    SqlStatement NewDeleteStatement(DataTable dataTable);

    SqlStatement NewInsertStatement(DataTable dataTable);

    SqlStatement NewSelectAllStatement(DataTable dataTable);
    SqlStatement NewUpdateStatement(DataTable dataTable);
}
