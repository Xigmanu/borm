using System.Data;

namespace Borm.Extensions;

internal static class DataRelationExtensions
{
    public static DataRelation Copy(this DataRelation original, DataSet dataSetCopy)
    {
        DataColumn parentColumn = GetColumn(original.ParentColumns[0], dataSetCopy);
        DataColumn childColumn = GetColumn(original.ChildColumns[0], dataSetCopy);

        return new DataRelation(original.RelationName, parentColumn, childColumn);
    }

    private static DataColumn GetColumn(DataColumn original, DataSet dataSetCopy)
    {
        DataTable table = dataSetCopy
            .Tables.Cast<DataTable>()
            .First(table => table.TableName == original.Table!.TableName);

        foreach (DataColumn column in table.Columns)
        {
            if (column.ColumnName == original.ColumnName)
            {
                return column;
            }
        }
        throw new InvalidOperationException(
            $"Table from copied data set does not have an expected column \"{original.ColumnName}\""
        );
    }
}
