using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;

namespace Borm.Model.Construction;

public sealed class ModelConfigurator
{
    private readonly HashSet<EntityInfo> _entities = [];
    private readonly ColumnValidatorFactoryContext _factoryContext = new();

    public IReadOnlyList<EntityInfo> Entities => _entities.ToList().AsReadOnly();

    public void FromBuilder<TEntity>(Action<EntityBuilder<TEntity>> builderConfigurator)
        where TEntity : class
    {
        EntityBuilder<TEntity> builder = new(
            new EntityConfigurationValidator<TEntity>(),
            _factoryContext
        );
        builderConfigurator(builder);
        AddEntity(builder.Build);
    }

    public void FromType<TEntity>()
        where TEntity : class
    {
        EntityFactory<TEntity> factory = new(
            new EntityConfigurationValidator<TEntity>(),
            _factoryContext
        );
        AddEntity(factory.Create);
    }

    private void AddEntity(Func<EntityInfo> entitySupplier)
    {
        EntityInfo entity = entitySupplier();
        if (!_entities.Add(entity))
        {
            throw new InvalidOperationException(
                $"Entity type \"{entity.Type.FullName}\" already exists."
            );
        }
    }
}