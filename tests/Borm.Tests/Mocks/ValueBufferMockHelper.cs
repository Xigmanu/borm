using Borm.Data.Storage;
using Borm.Model.Metadata;

namespace Borm.Tests.Mocks;

internal static class ValueBufferMockHelper
{
    public static readonly object[] AddressesDummyData = [1, "address", DBNull.Value, "city"];
    public static readonly object[] EmployeesDummyData = [1, 1, true];
    public static readonly object[] PersonsDummyData = [1, "name", 42.619, 1];

    public static ValueBuffer CreateBuffer(object[] values, Table table)
    {
        ValueBuffer buffer = new();

        ColumnMetadataCollection columns = table.Metadata.Columns;
        for (int i = 0; i < columns.Count; i++)
        {
            buffer[columns[i]] = values[i];
        }

        return buffer;
    }
}
