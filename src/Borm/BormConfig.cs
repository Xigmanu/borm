using Borm.Data.Sql;
using Borm.Model;

namespace Borm;

public sealed class BormConfig
{
    private BormConfig(
        IDbCommandExecutor commandExecutor,
        EntityModel model,
        IDbCommandDefinitionFactory commandDefinitionFactory,
        bool transactionWriteOnCommit
    )
    {
        CommandExecutor = commandExecutor;
        Model = model;
        CommandDefinitionFactory = commandDefinitionFactory;
        TransactionWriteOnCommit = transactionWriteOnCommit;
    }

    public IDbCommandDefinitionFactory CommandDefinitionFactory { get; }
    public IDbCommandExecutor CommandExecutor { get; }
    public EntityModel Model { get; }
    public bool TransactionWriteOnCommit { get; }

    public sealed class Builder
    {
        private IDbCommandExecutor? _commandExecutor;
        private IDbCommandDefinitionFactory? _commandFactory;
        private EntityModel? _model;
        private bool _txWriteOnCommit;

        public Builder()
        {
            _txWriteOnCommit = false;
        }

        public BormConfig Build()
        {
            if (_model == null)
            {
                throw new InvalidOperationException("Entity model must be provided");
            }
            if (_commandExecutor == null)
            {
                throw new InvalidOperationException("Command executor must be provided");
            }
            if (_commandFactory == null)
            {
                throw new InvalidOperationException("Command definition factory must be provided");
            }

            return new BormConfig(
                _commandExecutor,
                _model,
                _commandFactory,
                _txWriteOnCommit
            );
        }

        public Builder CommandDefinitionFactory(IDbCommandDefinitionFactory definitionFactory)
        {
            ArgumentNullException.ThrowIfNull(definitionFactory);
            _commandFactory = definitionFactory;
            return this;
        }

        public Builder CommandExecutor(IDbCommandExecutor commandExecutor)
        {
            ArgumentNullException.ThrowIfNull(commandExecutor);
            _commandExecutor = commandExecutor;
            return this;
        }

        public Builder Model(EntityModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            _model = model;
            return this;
        }

        public Builder TransactionWriteOnCommit()
        {
            _txWriteOnCommit = true;
            return this;
        }
    }
}
