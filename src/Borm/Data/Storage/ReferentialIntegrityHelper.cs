using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

internal sealed class ReferentialIntegrityHelper
{
    private readonly TableGraph _graph;

    public ReferentialIntegrityHelper(TableGraph graph)
    {
        _graph = graph;
    }

    public HashSet<Table> ApplyDeleteRules(Table table, object parentPrimaryKey, long txId)
    {
        HashSet<Table> affectedTables = [];
        IEnumerable<Table> children = _graph.GetChildren(table);
        foreach (Table child in children)
        {
            IEnumerable<ColumnMetadata> foreignKeys = child.Metadata.Columns.Where(c =>
                c.Reference is not null
            );
            foreach (ColumnMetadata foreignKey in foreignKeys)
            {
                IEnumerable<IValueBuffer> affectedRecords = FindChildBuffers(
                    child,
                    foreignKey,
                    parentPrimaryKey
                );
                foreach (IValueBuffer affectedBuffer in affectedRecords)
                {
                    ExecuteOnDeleteAction(child, foreignKey, affectedBuffer, txId);
                    affectedTables.Add(child);
                }
            }
        }

        return affectedTables;
    }

    private static void ExecuteOnDeleteAction(
        Table child,
        ColumnMetadata foreignKey,
        IValueBuffer childBuffer,
        long txId
    )
    {
        switch (foreignKey.OnDelete)
        {
            case ReferentialAction.Cascade:
                child.Delete(childBuffer, txId);
                break;
            case ReferentialAction.SetNull:
                childBuffer[foreignKey] = DBNull.Value;
                child.Update(childBuffer, txId);
                break;
            default:
                throw new NotSupportedException(
                    $"Unexpected {nameof(ReferentialAction)}: {foreignKey.OnDelete}"
                );
        }
    }

    private static IEnumerable<IValueBuffer> FindChildBuffers(
        Table child,
        ColumnMetadata foreignKey,
        object parentPrimaryKey
    )
    {
        return child
            .Tracker.Changes.Where(change => Equals(change.Buffer[foreignKey], parentPrimaryKey))
            .Select(change => change.Buffer.Copy());
    }
}
