using System.Reflection;
using Borm.Model.Metadata;

namespace Borm.Model.Validation.Expressions;

internal sealed class ColumnValidatorFactoryContext
{
    public ColumnValidatorFactoryContext()
    {
        Type vResultType = typeof(ValidationResult);
        VResultType = vResultType;
        ErrorMethod = vResultType.GetMethod(
            nameof(ValidationResult.Error),
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy
        )!;

        Type metaType = typeof(IEntityMetadata);
        EMetaName = metaType.GetProperty(nameof(IEntityMetadata.Name))!;
        EMetaGetColumn = metaType.GetMethod(nameof(IEntityMetadata.GetColumn))!;
        CMetaName = typeof(IColumnMetadata).GetProperty(nameof(IColumnMetadata.Name))!;
    }

    internal PropertyInfo CMetaName { get; }

    internal MethodInfo EMetaGetColumn { get; }

    internal PropertyInfo EMetaName { get; }
    internal MethodInfo ErrorMethod { get; }
    internal Type VResultType { get; }
}