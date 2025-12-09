using System.Collections.ObjectModel;
using System.Diagnostics;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

[DebuggerDisplay("TableCount = {TableCount}")]
internal sealed class TableGraph
{
    private readonly Dictionary<ITable, HashSet<ITable>> _children = [];
    private readonly Dictionary<ITable, HashSet<ITable>> _parents = [];
    private readonly HashSet<ITable> _tables = [];

    public int TableCount => _tables.Count;

    public ITable? this[Type entityType] =>
        _tables.FirstOrDefault(t => t.Metadata.Type == entityType);

    public void AddEdge(ITable parent, ITable child)
    {
        Debug.Assert(_tables.Contains(parent) && _tables.Contains(child) && !parent.Equals(child));

        InternalAddRelation(parent, child, _children);
        InternalAddRelation(child, parent, _parents);
    }

    public void AddTable(ITable table)
    {
        _ = _tables.Add(table);
    }

    public IEnumerable<ITable> GetChildren(ITable table)
    {
        return GetEdges(table, _children);
    }

    public IEnumerable<ITable> GetParents(ITable table)
    {
        return GetEdges(table, _parents);
    }

    public TableInfo GetTableSchema(ITable table)
    {
        List<ColumnInfo> columns = [];
        Dictionary<ColumnInfo, TableInfo> fkRelationMap = [];

        ColumnInfo? primaryKey = null;
        foreach (IColumnMetadata column in table.Metadata.Columns)
        {
            string columnName = column.Name;
            bool isUnique = column.Constraints.HasFlag(Constraints.Unique);
            bool isNullable = column.Constraints.HasFlag(Constraints.AllowDbNull);

            ColumnInfo columnInfo;
            if (column.Reference == null)
            {
                columnInfo = new ColumnInfo(
                    columnName,
                    column.DataType.UnderlyingType,
                    isUnique,
                    isNullable
                );
                columns.Add(columnInfo);

                if (column.Constraints.HasFlag(Constraints.PrimaryKey))
                {
                    primaryKey = columnInfo;
                }

                continue;
            }

            ITable? parent = this[column.Reference!];
            Debug.Assert(parent is not null);

            columnInfo = new ColumnInfo(
                columnName,
                parent.Metadata.PrimaryKey.DataType.UnderlyingType,
                isUnique,
                isNullable
            );
            TableInfo parentSchema = GetTableSchema(parent);

            columns.Add(columnInfo);
            fkRelationMap[columnInfo] = parentSchema;
        }

        Debug.Assert(primaryKey != null);
        return new TableInfo(
            table.Metadata.Name,
            new ReadOnlyCollection<ColumnInfo>(columns),
            primaryKey,
            fkRelationMap.AsReadOnly()
        );
    }

    public IEnumerable<ITable> TopSort()
    {
        List<ITable> result = [];
        HashSet<ITable> visited = [];

        foreach (ITable table in _tables)
        {
            Visit(table);
        }

        return result;

        void Visit(ITable table)
        {
            if (!visited.Add(table))
            {
                return;
            }

            if (_parents.TryGetValue(table, out HashSet<ITable>? parents))
            {
                foreach (ITable parent in parents)
                {
                    Visit(parent);
                }
            }

            result.Add(table);
        }
    }

    private static HashSet<ITable> GetEdges(
        ITable table,
        Dictionary<ITable, HashSet<ITable>> edgeMap
    )
    {
        if (edgeMap.TryGetValue(table, out HashSet<ITable>? successors))
        {
            return successors;
        }

        return [];
    }

    private static void InternalAddRelation(
        ITable from,
        ITable to,
        Dictionary<ITable, HashSet<ITable>> edgeMap
    )
    {
        if (edgeMap.TryGetValue(from, out HashSet<ITable>? successors))
        {
            _ = successors.Add(to);
            return;
        }

        successors = [to];
        edgeMap[from] = successors;
    }
}