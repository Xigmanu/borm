namespace Borm.Schema.Metadata;

internal static class EntityNodeGraphFactory
{
    public static EntityNodeGraph Create(IEnumerable<EntityNode> entityNodes)
    {
        EntityNodeGraph nodeGraph = new();
        foreach (EntityNode node in entityNodes)
        {
            IEnumerable<Type> referenced = node
                .Columns.Where(column => column.Reference != null)
                .Select(column => column.Reference!);
            if (!referenced.Any())
            {
                nodeGraph.AddSuccessorSet(node, []);
                continue;
            }

            List<EntityNode> successors =
            [
                .. entityNodes.Where(node => referenced.Contains(node.DataType)),
            ];

            nodeGraph.AddSuccessorSet(node, successors);
        }

        return nodeGraph;
    }
}
