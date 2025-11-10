using System.Diagnostics;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Validators;

internal sealed class ModelRelationsValidator : IValidator<IReadOnlyList<EntityInfo>>
{
    public void Validate(IReadOnlyList<EntityInfo> model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Debug.Assert(model.Count != 0);

        foreach ((_, Type entityType, IReadOnlyList<MappingMember> readOnlyList, _, _) in model)
        {
            foreach (MappingMember property in readOnlyList)
            {
                ValidateProperty(entityType.FullName ?? entityType.Name, property, model);
            }
        }
    }

    private static void ValidateProperty(
        string entityTypeName,
        MappingMember property,
        IReadOnlyList<EntityInfo> model
    )
    {
        MappingInfo? mapping = property.Mapping;
        Type propType = property.Type.UnderlyingType;
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

        MappingMember parentPK = parent.Properties.First(p => p.Mapping!.IsPrimaryKey);
        bool isTypeValid =
            propType.Equals(parent.Type) || propType.Equals(parentPK.Type.UnderlyingType);

        if (!isTypeValid)
        {
            throw new InvalidOperationException(Strings.InvalidForeignKeyDataType(entityTypeName));
        }
    }
}