using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Tests.Mocks.Builders;

namespace Borm.Tests.Mocks;

internal static class ValueBufferMockFactory
{
    public static readonly object[] AddressesDummyData = [1, "address", DBNull.Value, "city"];
    public static readonly object[] EmployeesDummyData = [1, 1, true];
    public static readonly object[] PersonsDummyData = [1, "name", 42.619, 1];

    public static IValueBuffer CreateBuffer(Dictionary<IColumnMetadata, object> columnValues)
    {
        return new ValueBufferImplBuilder()
            .ColumnValues(columnValues)
            .PrimaryKey(
                columnValues
                    .FirstOrDefault(kvp =>
                        kvp.Key.Constraints.HasFlag(Borm.Model.Constraints.PrimaryKey)
                    )!
                    .Value
            )
            .Build();
    }

    public static Dictionary<IColumnMetadata, object> MapValuesToColumns(
        object[] rowValues,
        IEntityMetadata metadata
    )
    {
        IReadOnlyList<IColumnMetadata> columns = metadata.Columns;
        Dictionary<IColumnMetadata, object> columnValues = [];
        for (int i = 0; i < rowValues.Length; i++)
        {
            columnValues[columns[i]] = rowValues[i];
        }

        return columnValues;
    }
}
