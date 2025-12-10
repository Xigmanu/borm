using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Borm.Reflection;

namespace Borm.Model.Validation.Expressions;

internal sealed class ValidationExpressionBuilder : IValidationExpressionBuilder
{
    private readonly ColumnValidatorFactoryContext _context;

    public ValidationExpressionBuilder(ColumnValidatorFactoryContext context)
    {
        _context = context;
    }

    public bool TryBuild(
        IMappable property,
        ParameterExpression entity,
        ParameterExpression metadata,
        ParameterExpression result,
        [NotNullWhen(true)] out Expression? validation
    )
    {
        validation = null;
        ValidationInfo? info = property.ValidationInfo;
        if (info == null)
        {
            return false;
        }

        (Expression condition, Expression propAccess) = info.AdjustToCommonParameter(entity);
        MemberExpression nameProperty = Expression.Property(metadata, _context.MetaName);

        MethodCallExpression getColCall = Expression.Call(
            metadata,
            _context.EMetaGetColumn,
            Expression.Constant(property.MemberName)
        );
        MemberExpression cName = Expression.Property(getColCall, _context.MetaName);

        MethodCallExpression errorCall = Expression.Call(
            _context.ErrorMethod,
            Expression.Convert(propAccess, typeof(object)),
            nameProperty,
            cName
        );

        validation = Expression.IfThen(
            Expression.Not(condition),
            Expression.Assign(result, errorCall)
        );
        return true;
    }
}