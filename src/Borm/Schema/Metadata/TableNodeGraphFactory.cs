namespace Borm.Schema.Metadata;

internal sealed class TableNodeGraphFactory
{
    private readonly IEnumerable<Type> _entityTypes;

    public TableNodeGraphFactory(IEnumerable<Type> entityTypes)
    {
        _entityTypes = entityTypes;
    }

    public TableNodeGraph Create()
    {
        List<TableNode> nodes = [];

        foreach (Type entityType in _entityTypes)
        {
            TableNode node = new TableNodeFactory(entityType).Create();
            nodes.Add(node);
        }

        TableNodeGraph nodeGraph = new();
        TableNodeValidator validator = new(nodes);
        foreach (TableNode node in nodes)
        {
            if (!validator.IsValid(node, out Exception? exception))
            {
                throw exception;
            }

            IEnumerable<Type> referenced = node
                .Columns.Where(column => column.ReferencedEntityType != null)
                .Select(column => column.ReferencedEntityType!);
            List<TableNode> successors =
            [
                .. nodes.Where(node => referenced.Contains(node.DataType)),
            ];

            nodeGraph.AddSuccessorSet(node, successors);
        }

        return nodeGraph;
    }
}
