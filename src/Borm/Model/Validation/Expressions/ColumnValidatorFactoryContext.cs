using System.Reflection;
using Borm.Model.Metadata;

namespace Borm.Model.Validation.Expressions;

internal sealed class ColumnValidatorFactoryContext
{
    public ColumnValidatorFactoryContext()
    {
        Type vResultType = typeof(ValidationResult);

        ErrorMethod = vResultType.GetMethod(
            nameof(ValidationResult.Error),
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy
        )!;
        OkMethod = vResultType.GetMethod(
            nameof(ValidationResult.Ok),
            BindingFlags.Public | BindingFlags.Static
        )!;
        VResultType = vResultType;

        Type metaType = typeof(IEntityMetadata);
        EMetaName = metaType.GetProperty(nameof(IEntityMetadata.Name))!;
        EMetaGetColumn = metaType.GetMethod(nameof(IEntityMetadata.GetColumn))!;
        CMetaName = typeof(IColumnMetadata).GetProperty(nameof(IColumnMetadata.Name))!;
    }

    internal PropertyInfo CMetaName { get; }
    internal MethodInfo EMetaGetColumn { get; }
    internal PropertyInfo EMetaName { get; }
    internal MethodInfo ErrorMethod { get; }
    internal MethodInfo OkMethod { get; }
    internal Type VResultType { get; }
}