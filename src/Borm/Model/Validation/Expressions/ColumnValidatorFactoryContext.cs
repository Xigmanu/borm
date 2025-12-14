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

        EMetaGetColumn = typeof(IEntityMetadata).GetMethod(nameof(IEntityMetadata.GetColumn))!;
        MetaName = typeof(IMetadata).GetProperty(nameof(IMetadata.Name))!;
    }

    internal MethodInfo EMetaGetColumn { get; }
    internal MethodInfo ErrorMethod { get; }
    internal PropertyInfo MetaName { get; }
    internal MethodInfo OkMethod { get; }
    internal Type VResultType { get; }
}