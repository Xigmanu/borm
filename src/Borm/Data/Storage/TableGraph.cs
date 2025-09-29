using System.Collections.ObjectModel;
using System.Diagnostics;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

[DebuggerDisplay("TableCount = {TableCount}")]
internal sealed class TableGraph
{
    private readonly Dictionary<Table, HashSet<Table>> _children;
    private readonly Dictionary<Table, HashSet<Table>> _parents;
    private readonly HashSet<Table> _tables;

    public TableGraph()
    {
        (_tables, _parents, _children) = ([], [], []);
    }

    public int TableCount => _tables.Count;

    public Table? this[Type entityType] =>
        _tables.FirstOrDefault(t => t.Metadata.DataType.Equals(entityType));

    public void AddEdge(Table parent, Table child)
    {
        Debug.Assert(_tables.Contains(parent) && _tables.Contains(child) && !parent.Equals(child));

        InternalAddRelation(parent, child, _children);
        InternalAddRelation(child, parent, _parents);
    }

    public void AddTable(Table table)
    {
        _ = _tables.Add(table);
    }

    public IEnumerable<Table> GetChildren(Table table)
    {
        return GetEdges(table, _children);
    }

    public IEnumerable<Table> GetParents(Table table)
    {
        return GetEdges(table, _parents);
    }

    public TableInfo GetTableSchema(Table table)
    {
        List<ColumnInfo> columns = [];
        Dictionary<ColumnInfo, TableInfo> fkRelationMap = [];

        ColumnInfo? primaryKey = null;
        foreach (ColumnMetadata column in table.Metadata.Columns)
        {
            string columnName = column.Name;
            bool isUnique = column.Constraints.HasFlag(Constraints.Unique);
            bool isNullable = column.Constraints.HasFlag(Constraints.AllowDbNull);

            ColumnInfo columnInfo;
            if (column.Reference == null)
            {
                columnInfo = new(columnName, column.DataType, isUnique, isNullable);
                columns.Add(columnInfo);

                if (column.Constraints.HasFlag(Constraints.PrimaryKey))
                {
                    primaryKey = columnInfo;
                }
                continue;
            }

            Table? parent = this[column.Reference!];
            Debug.Assert(parent is not null);

            columnInfo = new(columnName, parent.Metadata.PrimaryKey.DataType, isUnique, isNullable);
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

    public IEnumerable<Table> TopSort()
    {
        List<Table> result = [];
        HashSet<Table> visited = [];

        void Visit(Table table)
        {
            if (!visited.Add(table))
            {
                return;
            }

            if (_parents.TryGetValue(table, out HashSet<Table>? parents))
            {
                foreach (Table parent in parents)
                {
                    Visit(parent);
                }
            }

            result.Add(table);
        }

        foreach (Table table in _tables)
        {
            Visit(table);
        }

        return result;
    }

    private static HashSet<Table> GetEdges(Table table, Dictionary<Table, HashSet<Table>> edgeMap)
    {
        if (edgeMap.TryGetValue(table, out HashSet<Table>? successors))
        {
            return successors;
        }

        return [];
    }

    private static void InternalAddRelation(
        Table from,
        Table to,
        Dictionary<Table, HashSet<Table>> edgeMap
    )
    {
        if (edgeMap.TryGetValue(from, out HashSet<Table>? successors))
        {
            _ = successors.Add(to);
            return;
        }

        successors = [to];
        edgeMap[from] = successors;
    }
}
