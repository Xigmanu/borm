using System.Diagnostics;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Validation;

internal sealed class ModelRelationsValidator : IConfigurationValidator<IReadOnlyList<EntityInfo>>
{
    public void Validate(IReadOnlyList<EntityInfo> model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Debug.Assert(model.Count != 0);

        foreach (EntityInfo entity in model)
        {
            Type entityType = entity.Type;
            foreach (IMappable property in entity.Properties)
            {
                ValidateProperty(entityType.FullName ?? entityType.Name, property, model);
            }
        }
    }

    private static void ValidateProperty(
        string entityTypeName,
        IMappable property,
        IReadOnlyList<EntityInfo> model
    )
    {
        MappingInfo? mapping = property.Mapping;
        Type propType = property.DataType.UnderlyingType;
        Debug.Assert(mapping != null);
        if (mapping.Reference == null)
        {
            return;
        }

        Type reference = mapping.Reference;
        EntityInfo parent =
            model.FirstOrDefault(e => e.Type == reference)
            ?? throw new EntityNotFoundException(
                Strings.EntityDependencyNotFound(reference.FullName ?? reference.Name),
                reference
            );

        IMappable parentPk = parent.Properties.First(p => p.Mapping!.IsPrimaryKey);
        bool isTypeValid = propType == parent.Type || propType == parentPk.DataType.UnderlyingType;

        if (!isTypeValid)
        {
            throw new InvalidOperationException(Strings.InvalidForeignKeyDataType(entityTypeName));
        }
    }
}