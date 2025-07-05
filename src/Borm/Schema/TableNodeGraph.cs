namespace Borm.Schema;

internal sealed class TableNodeGraph
{
    private readonly Dictionary<TableNode, List<TableNode>> _adjacencyList;

    public TableNodeGraph()
    {
        _adjacencyList = [];
    }

    public IEnumerable<TableNode> Nodes => _adjacencyList.Keys;

    public TableNode? this[Type entityType]
    {
        get => _adjacencyList.Keys.FirstOrDefault(node => node.DataType.Equals(entityType));
    }

    public void AddSuccessorSet(TableNode node, List<TableNode> successors)
    {
        _adjacencyList[node] = successors;
    }

    public TableNode[] GetSuccessors(TableNode node)
    {
        if (_adjacencyList.TryGetValue(node, out List<TableNode>? successors))
        {
            return [.. successors];
        }
        throw new ArgumentException($"Node {node} was not found in the graph");
    }

    public TableNode[] ReversedTopSort()
    {
        Stack<TableNode> resultStack = [];
        HashSet<TableNode> visited = [];

        foreach (KeyValuePair<TableNode, List<TableNode>> nodeAdjPair in _adjacencyList)
        {
            TableNode node = nodeAdjPair.Key;
            if (!visited.Contains(node))
            {
                RecursiveTopSort(node, nodeAdjPair.Value, visited, resultStack);
            }
        }

        TableNode[] resultArray = new TableNode[resultStack.Count];
        for (int i = 0; i < resultArray.Length; i++)
        {
            resultArray[i] = resultStack.Pop();
        }
        Array.Reverse(resultArray);
        return resultArray;
    }

    private void RecursiveTopSort(
        TableNode node,
        List<TableNode> adj,
        HashSet<TableNode> visited,
        Stack<TableNode> resultStack
    )
    {
        visited.Add(node);
        foreach (TableNode successor in adj)
        {
            if (!visited.Contains(successor))
            {
                RecursiveTopSort(successor, _adjacencyList[successor], visited, resultStack);
            }
        }
        resultStack.Push(node);
    }
}
