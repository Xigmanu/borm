using System.Diagnostics;
using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;

namespace Borm.Data.Sql;

internal sealed class CommandBuilder
{
    private readonly IDbCommandDefinitionFactory _commandFactory;
    private readonly ITableGraph _graph;

    public CommandBuilder(ITableGraph graph, IDbCommandDefinitionFactory commandFactory)
    {
        _graph = graph;
        _commandFactory = commandFactory;
    }

    public IReadOnlyList<DbCommandDefinition> BuildUpdateCommands(ITable table)
    {
        IEnumerable<IChange> changes = table.Tracker.Changes;
        if (!changes.Any())
        {
            return [];
        }

        TableInfo schema = _graph.GetSchema(table);
        Dictionary<OperationKind, DbCommandDefinition> commandCache = [];

        foreach (IChange change in changes)
        {
            OperationKind operation = change.Operation;
            DbCommandDefinition? command = change.Operation switch
            {
                OperationKind.Insert => GetOrCreate(
                    schema,
                    operation,
                    commandCache,
                    _commandFactory.Insert
                ),
                OperationKind.Update => GetOrCreate(
                    schema,
                    operation,
                    commandCache,
                    _commandFactory.Update
                ),
                OperationKind.Delete => GetOrCreate(
                    schema,
                    operation,
                    commandCache,
                    _commandFactory.Delete
                ),
                _ => null
            };

            if (command == null)
            {
                continue;
            }

            Debug.Assert(!string.IsNullOrEmpty(command.Sql));
            command.BatchQueue.Enqueue(change.Record);
        }

        return commandCache.Values.ToList();
    }

    [DebuggerStepThrough]
    private static DbCommandDefinition GetOrCreate(
        TableInfo schema,
        OperationKind operation,
        Dictionary<OperationKind, DbCommandDefinition> commandCache,
        Func<TableInfo, DbCommandDefinition> factoryMethod
    )
    {
        if (commandCache.TryGetValue(operation, out DbCommandDefinition? command))
        {
            return command;
        }

        command = factoryMethod(schema);
        commandCache[operation] = command;

        return command;
    }
}