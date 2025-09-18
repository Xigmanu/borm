using Borm.Data;
using Borm.Data.Sql;
using Borm.Data.Storage;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Sql;

public sealed class CommandBuilderTest
{
    private const string CreateCmdFormat = "create {0}";
    private const string DeleteCmdFormat = "delete from {0}";
    private const string InsertCmdFormat = "insert into {0}";
    private const string SelectCmdFormat = "select from {0}";
    private const string UpdateCmdFormat = "update in {0}";

    private static readonly TestDbCommandDefinitionFactory CommandFactory = new();

    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void BuildUpdateCommands_BuildsCommandForTable_Delete()
    {
        // Arrange
        long initialTxId = -1;
        long txId = 0;
        CommandBuilder builder = new(_graph, CommandFactory);
        Table table = _graph[typeof(AddressEntity)]!;

        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        Change initial = Change.Initial(buffer, initialTxId);
        table.Tracker.PendChange(initial);
        table.Tracker.AcceptPendingChanges(initialTxId);

        table.Tracker.PendChange(initial.Delete(buffer, txId));
        table.Tracker.AcceptPendingChanges(txId);

        // Act
        IEnumerable<DbCommandDefinition> commands = builder.BuildUpdateCommands(table);

        // Assert
        Assert.Single(commands);

        DbCommandDefinition command = commands.First();
        Assert.Equal(string.Format(DeleteCmdFormat, table.Name), command.Sql);

        Assert.Equal(1, command.BatchQueue.Count);
    }

    [Fact]
    public void BuildUpdateCommands_BuildsCommandForTable_Insert()
    {
        // Arrange
        long txId = 0;
        CommandBuilder builder = new(_graph, CommandFactory);
        Table table = _graph[typeof(AddressEntity)]!;

        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        table.Tracker.PendChange(Change.NewChange(buffer, txId));
        table.Tracker.AcceptPendingChanges(txId);

        // Act
        IEnumerable<DbCommandDefinition> commands = builder.BuildUpdateCommands(table);

        // Assert
        Assert.Single(commands);

        DbCommandDefinition command = commands.First();
        Assert.Equal(string.Format(InsertCmdFormat, table.Name), command.Sql);

        Assert.Equal(1, command.BatchQueue.Count);
    }

    [Fact]
    public void BuildUpdateCommands_BuildsCommandForTable_Update()
    {
        // Arrange
        long initialTxId = -1;
        long txId = 0;
        CommandBuilder builder = new(_graph, CommandFactory);
        Table table = _graph[typeof(AddressEntity)]!;

        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        Change initial = Change.Initial(buffer, initialTxId);
        table.Tracker.PendChange(initial);
        table.Tracker.AcceptPendingChanges(initialTxId);

        table.Tracker.PendChange(initial.Update(buffer, txId));
        table.Tracker.AcceptPendingChanges(txId);

        // Act
        IEnumerable<DbCommandDefinition> commands = builder.BuildUpdateCommands(table);

        // Assert
        Assert.Single(commands);

        DbCommandDefinition command = commands.First();
        Assert.Equal(string.Format(UpdateCmdFormat, table.Name), command.Sql);

        Assert.Equal(1, command.BatchQueue.Count);
    }

    [Fact]
    public void BuildUpdateCommands_ReturnsEmptyEnumeration_WhenTableHasNoChanges()
    {
        // Arrange
        CommandBuilder builder = new(_graph, CommandFactory);
        Table table = _graph[typeof(AddressEntity)]!;

        // Act
        IEnumerable<DbCommandDefinition> commands = builder.BuildUpdateCommands(table);

        // Assert
        Assert.Empty(commands);
    }

    [Fact]
    public void BuildUpdateCommands_ReturnsEmptyEnumeration_WhenTableHasNoChangesToWrite()
    {
        // Arrange
        long initialTxId = -1;
        CommandBuilder builder = new(_graph, CommandFactory);
        Table table = _graph[typeof(AddressEntity)]!;

        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        table.Tracker.PendChange(Change.Initial(buffer, initialTxId));
        table.Tracker.AcceptPendingChanges(initialTxId);
        table.Tracker.MarkChangesAsWritten();

        // Act
        IEnumerable<DbCommandDefinition> commands = builder.BuildUpdateCommands(table);

        // Assert
        Assert.Empty(commands);
    }

    private sealed class TestDbCommandDefinitionFactory : IDbCommandDefinitionFactory
    {
        public DbCommandDefinition CreateTable(TableInfo tableSchema)
        {
            return new DbCommandDefinition(string.Format(CreateCmdFormat, tableSchema.Name), []);
        }

        public DbCommandDefinition Delete(TableInfo tableSchema)
        {
            return new DbCommandDefinition(string.Format(DeleteCmdFormat, tableSchema.Name), []);
        }

        public DbCommandDefinition Insert(TableInfo tableSchema)
        {
            return new DbCommandDefinition(string.Format(InsertCmdFormat, tableSchema.Name), []);
        }

        public DbCommandDefinition SelectAll(TableInfo tableSchema)
        {
            return new DbCommandDefinition(string.Format(SelectCmdFormat, tableSchema.Name), []);
        }

        public DbCommandDefinition Update(TableInfo tableSchema)
        {
            return new DbCommandDefinition(string.Format(UpdateCmdFormat, tableSchema.Name), []);
        }
    }
}
