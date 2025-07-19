using System.Data;

namespace Borm.Data.Sql;

public interface IDbStatementExecutor
{
    void ExecuteNonQuery(SqlStatement statement);
    IDataReader ExecuteReader(SqlStatement statement);
}
