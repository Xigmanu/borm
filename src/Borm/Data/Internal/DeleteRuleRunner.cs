using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Data.Internal;

internal sealed class DeleteRuleRunner : IReferentialActionExecutor
{
    private readonly TableGraph _graph;

    public DeleteRuleRunner(TableGraph graph)
    {
        _graph = graph;
    }

    public ISet<ITable> Run(ITable table, object parentPk, long txId)
    {
        HashSet<ITable> affected = [];
        foreach (ITable child in _graph.GetChildren(table))
        {
            IEnumerable<IColumnMetadata> foreignKeys = child.Metadata.Columns.Where(c =>
                c.Reference is not null
            );
            foreach (IColumnMetadata foreignKey in foreignKeys)
            {
                IEnumerable<IValueBuffer> affectedRecords = FindChildrenBuffers(
                    child,
                    foreignKey,
                    parentPk
                );
                foreach (IValueBuffer affectedBuffer in affectedRecords)
                {
                    ExecuteOnDeleteAction(child, foreignKey, affectedBuffer, txId);
                    affected.Add(child);
                }
            }
        }

        return affected;
    }

    private static void ExecuteOnDeleteAction(
        ITable child,
        IColumnMetadata foreignKey,
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
            case ReferentialAction.NoAction:
            default:
                throw new NotSupportedException(
                    $"Unexpected {nameof(ReferentialAction)}: {foreignKey.OnDelete}"
                );
        }
    }

    private static IEnumerable<IValueBuffer> FindChildrenBuffers(
        ITable child,
        IColumnMetadata foreignKey,
        object parentPrimaryKey
    )
    {
        return child
            .Tracker.Changes.Where(change => Equals(change.Record[foreignKey], parentPrimaryKey))
            .Select(change => change.Record.Copy());
    }
}