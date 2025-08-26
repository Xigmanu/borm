using System.Diagnostics;
using Borm.Data.Sql;
using Borm.Data.Storage;

namespace Borm.Data;

internal sealed class ChangeCommandBuilder
{
    private readonly Dictionary<RowAction, DbCommandDefinition> _commandCache;
    private readonly IDbCommandDefinitionFactory _commandFactory;
    private readonly Table _table;

    public ChangeCommandBuilder(Table table, IDbCommandDefinitionFactory commandFactory)
    {
        _commandFactory = commandFactory;
        _table = table;
        _commandCache = [];
    }

    public IEnumerable<DbCommandDefinition> BuildUpdateCommands()
    {
        IEnumerable<Change> changes = _table.Tracker.Changes;
        if (!changes.Any())
        {
            return [];
        }

        TableInfo schema = _table.GetTableSchema();

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
