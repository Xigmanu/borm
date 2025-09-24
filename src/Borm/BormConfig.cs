using Borm.Data.Sql;
using Borm.Model;

namespace Borm;

/// <summary>
/// Configuration to be used to initialize <see cref="DataContext"/>.
/// </summary>
public sealed class BormConfig
{
    private BormConfig(
        IDbCommandExecutor commandExecutor,
        EntityModel model,
        IDbCommandDefinitionFactory commandDefinitionFactory
    )
    {
        CommandExecutor = commandExecutor;
        Model = model;
        CommandDefinitionFactory = commandDefinitionFactory;
    }

    /// <summary>
    /// Factory responsible for creating <see cref="DbCommandDefinition"/> instances
    /// for database operations.
    /// </summary>
    public IDbCommandDefinitionFactory CommandDefinitionFactory { get; }

    /// <summary>
    /// Executor responsible for executing database commands.
    /// </summary>
    public IDbCommandExecutor CommandExecutor { get; }

    /// <summary>
    /// Entity model to be used for table creation.
    /// </summary>
    public EntityModel Model { get; }

    /// <summary>
    /// Builder for constructing <see cref="BormConfig"/> instances.
    /// </summary>
    public sealed class Builder
    {
        private IDbCommandDefinitionFactory? _commandDefinitionFactory;
        private IDbCommandExecutor? _commandExecutor;
        private EntityModel? _model;

        /// <summary>
        /// Builds a new <see cref="BormConfig"/> instance using the values set on this builder.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if required components are not provided.</exception>
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
            if (_commandDefinitionFactory == null)
            {
                throw new InvalidOperationException("Command definition factory must be provided");
            }

            return new BormConfig(_commandExecutor, _model, _commandDefinitionFactory);
        }

        public Builder CommandDefinitionFactory(
            IDbCommandDefinitionFactory commandDefinitionFactory
        )
        {
            ArgumentNullException.ThrowIfNull(commandDefinitionFactory);
            _commandDefinitionFactory = commandDefinitionFactory;
            return this;
        }

        public Builder CommandExecutor(IDbCommandExecutor commandExecutor)
        {
            ArgumentNullException.ThrowIfNull(commandExecutor);
            _commandExecutor = commandExecutor;
            return this;
        }

        /// <summary>
        /// Configures the builder to use an in-memory database implementation.
        /// </summary>
        /// <returns></returns>
        public Builder InMemory()
        {
            _commandExecutor = new InMemoryCommandExecutor();
            _commandDefinitionFactory = new InMemoryCommandDefinitionFactory();
            return this;
        }

        public Builder Model(EntityModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            _model = model;
            return this;
        }
    }
}
