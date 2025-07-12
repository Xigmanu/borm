namespace Borm.Schema.Metadata;

internal sealed class EntityNodeGraphFactory
{
    private readonly IEnumerable<EntityNode> _entityNodes;

    public EntityNodeGraphFactory(IEnumerable<EntityNode> entityNodes)
    {
        _entityNodes = entityNodes;
    }

    public EntityNodeGraph Create()
    {
        EntityNodeGraph nodeGraph = new();
        foreach (EntityNode node in _entityNodes)
        {
            IEnumerable<Type> referenced = node
                .Columns.Where(column => column.Reference != null)
                .Select(column => column.Reference!);
            List<EntityNode> successors =
            [
                .. _entityNodes.Where(node => referenced.Contains(node.DataType)),
            ];

            nodeGraph.AddSuccessorSet(node, successors);
        }

        return nodeGraph;
    }
}
