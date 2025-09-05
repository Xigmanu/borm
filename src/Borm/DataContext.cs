using System.Diagnostics;
using Borm.Data;
using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Reflection;

namespace Borm;

/// <summary>
/// Represents a session with a database and can be used to create instances of <see cref="IEntityRepository{T}"/>
/// to provide read/write access to an entity table.
/// </summary>
/// 
/// <remarks>
///     <para>
///         Entity classes are public classes that are marked with the <see cref="EntityAttribute"/>.
///         Any properties in these classes that are to be used for mapping must be marked with the <see cref="ColumnAttribute"/>.
///         Entity classes are then registered using the <see cref="EntityModel"/> class as part of the data context configuration.
///     </para>
///     <para>
///         Instances of entity classes are created using either a public constructor or public setters.
///         For constructor binding, an entity class must contain a single constructor that initialises all
///         properties relevant for mapping (other properties or fields cannot be initialised using a constructor).
///         The parameters of the constructor must have the same name as the columns to which they assign a value.<br/>
///         Note: Constructor binding will automatically be used if an entity class contains an explicit constructor.For setter-based binding, do not define any constructors.
///     </para>
/// </remarks>
public sealed class DataContext
{
    private readonly BormConfig _configuration;
    private readonly DataSynchronizer _dataSynchronizer;
    private readonly TableGraph _tableGraph;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataContext"/> class with the specified configuration.
    /// </summary>
    /// 
    /// <param name="configuration">Configuration for the context.</param>
    public DataContext(BormConfig configuration)
    {
        _configuration = configuration;
        _tableGraph = new();
        _dataSynchronizer = new(
            configuration.CommandExecutor,
            _tableGraph,
            configuration.CommandDefinitionFactory
        );
    }

    /// <summary>
    /// An event fired at the end of a call to <see cref="Initialize"/>.
    /// </summary>
    public event EventHandler? Initialized;

    /// <summary>
    /// Begins a new transaction scope for changes performed through this context.
    /// </summary>
    /// 
    /// <remarks>
    ///     Transactions are disposable. Commits and rollbacks occur when the <see cref="O:Dispose"/> method is called.
    /// </remarks>
    /// <returns>A transaction for given data context.</returns>
    public Transaction BeginTransaction()
    {
        return new Transaction(this, _configuration.TransactionWriteOnCommit);
    }

    /// <summary>
    /// Creates a repository for the specified entity type.
    /// </summary>
    /// 
    /// <typeparam name="T">The entity type registered in this data context.</typeparam>
    /// <returns>An <see cref="IEntityRepository{T}"/> for the specified entity type.</returns>
    /// <exception cref="InvalidOperationException">The <see cref="Initialize"/> was not called prior to calling this method.</exception>
    /// <exception cref="ArgumentException">Provided generic argument does not match any entity type registered in this data context.</exception>
    public IEntityRepository<T> GetRepository<T>()
        where T : class
    {
        if (_tableGraph == null)
        {
            throw new InvalidOperationException(Strings.DataContextNotInitialized());
        }

        Type entityType = typeof(T);
        Table table =
            _tableGraph[entityType]
            ?? throw new ArgumentException(Strings.MissingTableForEntity(entityType.FullName!));
        Debug.Assert(table != null);

        return new EntityRepository<T>(table);
    }

    /// <summary>
    /// Initializes this data context using the provided configuration.
    /// </summary>
    /// 
    /// <remarks>
    ///     This method must be invoked prior to calling <see cref="GetRepository{T}"/>, <see cref="SaveChanges"/> or <see cref="SaveChangesAsync"/>.
    /// </remarks>
    public void Initialize()
    {
        EntityModel model = _configuration.Model;
        IEnumerable<ReflectedTypeInfo> typeInfos = model.GetReflectedInfo();
        if (!typeInfos.Any())
        {
            return;
        }

        List<EntityMetadata> entityInfos = new(typeInfos.Count());
        foreach (ReflectedTypeInfo typeInfo in typeInfos)
        {
            EntityMetadata entityMetadata = EntityMetadataBuilder.Build(typeInfo);

            EntityMaterializationBinding binding = new(typeInfo.Type, entityMetadata.Columns);
            entityMetadata.Binding = binding.CreateBinding();
            entityMetadata.Validator = model.GetValidatorFunc(typeInfo.Type);

            entityInfos.Add(entityMetadata);
        }

        EntityInfoValidator validator = new(entityInfos);
        entityInfos.ForEach(info =>
        {
            if (!validator.IsValid(info, out Exception? exception))
            {
                throw exception;
            }
        });

        IEnumerable<Table> tables = new TableGraphBuilder(entityInfos).BuildAll();
        _tableGraph.AddTableRange(tables);

        _dataSynchronizer.SyncSchemaWithDataSource();

        OnInitialized();
    }

    /// <summary>
    /// Writes all changes made to this context since it was initialized
    /// or since the last time this method was called.
    /// </summary>
    public void SaveChanges()
    {
        _dataSynchronizer.SaveChanges();
    }

    /// <summary>
    /// Asynchronously writes all changes made to this context since it was initialized
    /// or since the last time this method was called.
    /// </summary>
    public Task SaveChangesAsync()
    {
        return _dataSynchronizer.SaveChangesAsync();
    }

    private void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }
}
