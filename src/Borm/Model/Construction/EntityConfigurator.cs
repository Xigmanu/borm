using Borm.Model.Validators;

namespace Borm.Model.Construction;

public static class EntityConfigurator<T>
    where T : class
{
    public static EntityBuilder<T> Builder()
    {
        return new EntityBuilder<T>(new EntityConfigurationValidator<T>());
    }

    public static EntityInfo FromType()
    {
        return EntityFactory<T>.Create(new EntityConfigurationValidator<T>());
    }

    public static EntityInfo FromType(IValidator<T> validator)
    {
        return EntityFactory<T>.Create(new EntityConfigurationValidator<T>(), validator);
    }
}