namespace Borm.Schema.Metadata;

internal sealed class EntityNodeGraph
{
    private readonly Dictionary<EntityNode, List<EntityNode>> _adjacencyList;

    public EntityNodeGraph()
    {
        _adjacencyList = [];
    }

    public IEnumerable<EntityNode> Nodes => _adjacencyList.Keys;

    public EntityNode? this[Type entityType]
    {
        get => _adjacencyList.Keys.FirstOrDefault(node => node.DataType.Equals(entityType));
    }

    public void AddSuccessorSet(EntityNode node, List<EntityNode> successors)
    {
        _adjacencyList[node] = successors;
    }

    public EntityNode[] GetSuccessors(EntityNode node)
    {
        if (_adjacencyList.TryGetValue(node, out List<EntityNode>? successors))
        {
            return [.. successors];
        }
        throw new ArgumentException($"Node {node} was not found in the graph");
    }

    public EntityNode[] ReversedTopSort()
    {
        Stack<EntityNode> resultStack = [];
        HashSet<EntityNode> visited = [];

        foreach (KeyValuePair<EntityNode, List<EntityNode>> nodeAdjPair in _adjacencyList)
        {
            EntityNode node = nodeAdjPair.Key;
            if (!visited.Contains(node))
            {
                RecursiveTopSort(node, nodeAdjPair.Value, visited, resultStack);
            }
        }

        EntityNode[] resultArray = new EntityNode[resultStack.Count];
        for (int i = 0; i < resultArray.Length; i++)
        {
            resultArray[i] = resultStack.Pop();
        }
        Array.Reverse(resultArray);
        return resultArray;
    }

    private void RecursiveTopSort(
        EntityNode node,
        List<EntityNode> adj,
        HashSet<EntityNode> visited,
        Stack<EntityNode> resultStack
    )
    {
        visited.Add(node);
        foreach (var successor in adj.Where(successor => !visited.Contains(successor)))
        {
            RecursiveTopSort(successor, _adjacencyList[successor], visited, resultStack);
        }

        resultStack.Push(node);
    }
}
