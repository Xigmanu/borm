using Borm.Extensions;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model;

public sealed class EntityModel
{
    private readonly HashSet<Type> _entities;
    private readonly Dictionary<Type, Action<object>> _entityValidators;

    public EntityModel()
    {
        (_entities, _entityValidators) = ([], []);
    }

    public void AddEntity(Type entityType)
    {
        if (!entityType.HasAttribute<EntityAttribute>())
        {
            throw new ArgumentException(Strings.NotDecoratedEntityType(), nameof(entityType));
        }
        _entities.Add(entityType);
    }

    public void AddEntity<TEntity>(Type entityType, IEntityValidator<TEntity> validator)
    {
        AddEntity(entityType);
        _entityValidators[typeof(TEntity)] = WrapValidator(validator);
    }

    internal IEnumerable<ReflectedTypeInfo> GetReflectedInfo()
    {
        List<ReflectedTypeInfo> reflectedInfos = new(_entities.Count);
        MetadataParser parser = new();
        foreach (Type entityType in _entities)
        {
            if (entityType.IsAbstract)
            {
                throw new ArgumentException(
                    Strings.EntityTypeCannotBeAbstract(entityType.FullName!)
                );
            }

            ReflectedTypeInfo reflectedInfo = parser.Parse(entityType);
            reflectedInfos.Add(reflectedInfo);
        }

        return reflectedInfos;
    }

    internal Action<object>? GetValidatorFunc(Type entityType)
    {
        _ = _entityValidators.TryGetValue(entityType, out Action<object>? validator);
        return validator;
    }

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
