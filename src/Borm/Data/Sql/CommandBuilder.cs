using System.Diagnostics;
using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;

namespace Borm.Data.Sql;

internal sealed class CommandBuilder
{
    private readonly Dictionary<RowAction, DbCommandDefinition> _commandCache;
    private readonly TableGraph _graph;
    private readonly IDbCommandDefinitionFactory _commandFactory;

    public CommandBuilder(TableGraph graph, IDbCommandDefinitionFactory commandFactory)
    {
        _graph = graph;
        _commandFactory = commandFactory;
        _commandCache = [];
    }

    public IEnumerable<DbCommandDefinition> BuildUpdateCommands(Table table)
    {
        IEnumerable<Change> changes = table.Tracker.Changes;
        if (!changes.Any())
        {
            return [];
        }

        TableInfo schema = _graph.GetTableSchema(table);

        foreach (Change change in changes)
        {
            RowAction action = change.RowAction;
            DbCommandDefinition? command = change.RowAction switch
            {
                RowAction.Insert => GetOrCreate(schema, action, _commandFactory.Insert),
                RowAction.Update => GetOrCreate(schema, action, _commandFactory.Update),
                RowAction.Delete => GetOrCreate(schema, action, _commandFactory.Delete),
                _ => null,
            };

            if (command != null)
            {
                Debug.Assert(!string.IsNullOrEmpty(command.Sql));
                command.BatchQueue.Enqueue(change.Buffer);
            }
        }

        return _commandCache.Values;
    }

    [DebuggerStepThrough]
    private DbCommandDefinition GetOrCreate(
        TableInfo schema,
        RowAction action,
        Func<TableInfo, DbCommandDefinition> factoryMethod
    )
    {
        if (!_commandCache.TryGetValue(action, out DbCommandDefinition? command))
        {
            command = factoryMethod(schema);
            _commandCache[action] = command;
        }

        return command;
    }
}
