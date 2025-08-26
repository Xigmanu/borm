using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Borm.Data.Sql;

namespace Borm.Extensions;

public static class DbDataReaderExtensions
{
    public static ResultSet ToResultSet(this DbDataReader reader)
    {
        Debug.Assert(!reader.IsClosed);

        ResultSet resultSet = new();
        IEnumerable<string> columnNames = reader.GetColumnSchema().Select(c => c.ColumnName);

        while (reader.Read())
        {
            Dictionary<string, object> row = new(StringComparer.OrdinalIgnoreCase);
            foreach (string column in columnNames)
            {
                row[column] = reader[column];
            }
            resultSet.AddRow(row);
        }

        return resultSet;
    }
}
