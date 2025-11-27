using Borm.Model.Validation;
using Borm.Reflection;

namespace Borm.Model.Construction;

public static class EntityConfigurator<T>
    where T : class
{
    private static readonly IConfigurationValidator<IReadOnlyList<MappingMember>> ConfigurationValidator =
        new EntityConfigurationValidator<T>();

    public static EntityBuilder<T> Builder() => new(ConfigurationValidator);

    public static EntityInfo FromType() => EntityFactory<T>.Create(ConfigurationValidator);
}