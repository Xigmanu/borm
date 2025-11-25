using System.Linq.Expressions;
using System.Reflection;
using Borm.Model.Validation.Expressions.Visitors;
using Borm.Reflection;

namespace Borm.Model.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public abstract class ValidWhenAttribute : Attribute
{
    protected abstract Expression<Func<object, bool>> ValidWhen { get; }

    internal ValidatorExpressionInfo GetValidatorInfo<TEntity>(PropertyInfo property)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");
        MemberExpression propAccess = Expression.Property(parameter, property);
        ParameterExpressionRemapper remapper = new(ValidWhen.Parameters[0], propAccess);
        LambdaExpression lambda = Expression.Lambda<Func<TEntity, bool>>(
            remapper.Visit(ValidWhen.Body),
            parameter
        );

        return new ValidatorExpressionInfo(lambda, propAccess);
    }
}