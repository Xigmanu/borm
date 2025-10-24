using Borm.Extensions;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model;

/// <summary>
/// Represents the collection of entity types and their associated metadata.
/// </summary>
public sealed class EntityModel
{
    private readonly HashSet<Type> _entities;
    private readonly Dictionary<Type, Action<object>> _entityValidators;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityModel"/> class.
    /// </summary>
    public EntityModel()
    {
        (_entities, _entityValidators) = ([], []);
    }

    /// <summary>
    /// Adds an entity type to the model.
    /// </summary>
    /// <param name="entityType">The entity type to register.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="entityType"/> is not decorated with <see cref="EntityAttribute"/>.
    /// </exception>
    public void AddEntity(Type entityType)
    {
        if (!entityType.HasAttribute<EntityAttribute>())
        {
            throw new ArgumentException(Strings.NotDecoratedEntityType(), nameof(entityType));
        }
        _entities.Add(entityType);
    }

    /// <summary>
    /// Adds an entity type to the model and associates a validator for it.
    /// </summary>
    /// <param name="entityType">The entity type to register.</param>
    /// <param name="validator">
    /// The validator to use for validating instances of <typeparamref name="TEntity"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="entityType"/> is not decorated with <see cref="EntityAttribute"/>.
    /// </exception>
    public void AddEntity<TEntity>(Type entityType, IEntityValidator<TEntity> validator)
    {
        AddEntity(entityType);
        _entityValidators[typeof(TEntity)] = WrapValidator(validator);
    }

    internal IEnumerable<EntityTypeInfo> GetReflectedInfo()
    {
        List<EntityTypeInfo> reflectedInfos = new(_entities.Count);
        MetadataParser parser = new();
        foreach (Type entityType in _entities)
        {
            if (entityType.IsAbstract)
            {
                throw new ArgumentException(
                    Strings.EntityTypeCannotBeAbstract(entityType.FullName!)
                );
            }

            EntityTypeInfo reflectedInfo = parser.Parse(entityType, _entityValidators[entityType]);
            reflectedInfos.Add(reflectedInfo);
        }

        return reflectedInfos;
    }

    // TODO Implement a better validation logic
    private static Action<object> WrapValidator<TEntity>(IEntityValidator<TEntity> validator)
    {
        return (obj) =>
        {
            try
            {
                validator.Validate((TEntity)obj);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    Strings.EntityValidationFailed(typeof(TEntity)),
                    ex
                );
            }
        };
    }
}
