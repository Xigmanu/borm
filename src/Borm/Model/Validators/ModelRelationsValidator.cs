using System.Diagnostics;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Model.Validators;

internal sealed class ModelRelationsValidator : IValidator<IReadOnlyList<EntityInfo>>
{
    public void Validate(IReadOnlyList<EntityInfo> model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Debug.Assert(model.Count != 0);

        foreach (MappingMember property in model.SelectMany(e => e.Properties))
        {
            MappingInfo? mapping = property.Mapping;
            Type propType = property.Type.UnderlyingType;
            Debug.Assert(mapping != null);
            if (mapping.Reference == null)
            {
                continue;
            }

            EntityInfo? parent =
                model.FirstOrDefault(e => e.Type == mapping.Reference)
                ?? throw new EntityNotFoundException(
                    $"Referenced entity type {mapping.Reference.FullName} does not exist",
                    mapping.Reference
                );

            MappingMember parentPK = parent.Properties.First(p => p.Mapping!.IsPrimaryKey);
            bool isTypeValid =
                propType.Equals(parent.Type) || propType.Equals(parentPK.Type.UnderlyingType);

            if (!isTypeValid)
            {
                throw new InvalidOperationException(
                    $"The foreign key property must be of the referenced type or the type of its primary key. Entity: {parent.Type.FullName}"
                );
            }
        }
    }
}
