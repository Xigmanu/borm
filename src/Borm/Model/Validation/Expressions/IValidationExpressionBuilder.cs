using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Borm.Reflection;

namespace Borm.Model.Validation.Expressions;

internal interface IValidationExpressionBuilder
{
    bool TryBuild(
        IMappable property,
        ParameterExpression entity,
        ParameterExpression metadata,
        ParameterExpression result,
        [NotNullWhen(true)] out Expression? validation
    );
}