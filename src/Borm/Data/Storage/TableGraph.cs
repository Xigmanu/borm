using System.Diagnostics;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

[DebuggerDisplay("TableCount = {TableCount}")]
internal sealed class TableGraph
{
    private readonly Dictionary<Type, HashSet<Type>> _children;
    private readonly Dictionary<Type, HashSet<Type>> _parents;
    private readonly Dictionary<Type, Table> _tables;

    public TableGraph()
    {
        (_tables, _parents, _children) = ([], [], []);
    }

    public int TableCount => _tables.Count;

    public Table? this[Type entityType]
    {
        get
        {
            _ = _tables.TryGetValue(entityType, out Table? table);
            return table;
        }
    }

    public void AddChild(Table table, Table child)
    {
        InternalAddRelation(table, child, _tables, _children);
    }

    public void AddParent(Table table, Table parent)
    {
        InternalAddRelation(table, parent, _tables, _parents);
    }

    public void AddTable(Table table)
    {
        Type dataType = table.EntityMetadata.DataType;
        if (!_tables.ContainsKey(dataType))
        {
            _tables[dataType] = table;
            Debug.Assert(!_children.ContainsKey(dataType) && !_parents.ContainsKey(dataType));
        }
    }

    public void AddTableRange(IEnumerable<Table> tables)
    {
        foreach (Table table in tables)
        {
            AddTable(table);
        }
    }

    public IEnumerable<Table> GetChildren(Table table)
    {
        return GetEdges(table, _children);
    }

    public IEnumerable<Table> GetParents(Table table)
    {
        return GetEdges(table, _parents);
    }

    public TableInfo GetSchema(Table table)
    {
        List<ColumnInfo> columns = [];
        Dictionary<ColumnInfo, TableInfo> fkRelationMap = [];

        ColumnInfo? primaryKey = null;
        foreach (ColumnMetadata column in table.EntityMetadata.Columns)
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

            columnInfo = new(
                columnName,
                parent.EntityMetadata.PrimaryKey.DataType,
                isUnique,
                isNullable
            );
            TableInfo parentSchema = GetSchema(parent);

            fkRelationMap[columnInfo] = parentSchema;
        }

        Debug.Assert(primaryKey != null);
        return new TableInfo(table.EntityMetadata.Name, columns, primaryKey, fkRelationMap);
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

            foreach (Type parentDataType in _parents[table.EntityMetadata.DataType])
            {
                Visit(_tables[parentDataType]);
            }

            result.Add(table);
        }

        foreach (Table table in _tables.Values)
        {
            Visit(table);
        }

        return result;
    }

    private static void InternalAddRelation(
        Table source,
        Table target,
        Dictionary<Type, Table> tables,
        Dictionary<Type, HashSet<Type>> targetMap
    )
    {
        Debug.Assert(tables.ContainsValue(source) && !source.Equals(target));
        Type key = source.EntityMetadata.DataType;
        Type value = target.EntityMetadata.DataType;

        if (targetMap.TryGetValue(key, out HashSet<Type>? values))
        {
            bool added = values.Add(value);
            Debug.Assert(added);
            return;
        }

        values = [value];
        targetMap[key] = values;
    }

    private IEnumerable<Table> GetEdges(Table table, Dictionary<Type, HashSet<Type>> edgeMap)
    {
        if (edgeMap.TryGetValue(table.EntityMetadata.DataType, out HashSet<Type>? edges))
        {
            return edges.Select(type =>
            {
                Table? child = this[type];
                Debug.Assert(child is not null);
                return child;
            });
        }

        return [];
    }
}
