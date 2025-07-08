namespace Borm.Schema.Metadata;

internal sealed class EntityNodeGraphFactory
{
    private readonly IEnumerable<Type> _entityTypes;

    public EntityNodeGraphFactory(IEnumerable<Type> entityTypes)
    {
        _entityTypes = entityTypes;
    }

    public EntityNodeGraph Create()
    {
        List<EntityNode> nodes = [];

        foreach (Type entityType in _entityTypes)
        {
            EntityNode node = new EntityNodeFactory(entityType).Create();
            nodes.Add(node);
        }

        EntityNodeGraph nodeGraph = new();
        EntityNodeValidator validator = new(nodes);
        foreach (EntityNode node in nodes)
        {
            if (!validator.IsValid(node, out Exception? exception))
            {
                throw exception;
            }

            IEnumerable<Type> referenced = node
                .Columns.Where(column => column.ReferencedEntityType != null)
                .Select(column => column.ReferencedEntityType!);
            List<EntityNode> successors =
            [
                .. nodes.Where(node => referenced.Contains(node.DataType)),
            ];

            nodeGraph.AddSuccessorSet(node, successors);
        }

        return nodeGraph;
    }
}
