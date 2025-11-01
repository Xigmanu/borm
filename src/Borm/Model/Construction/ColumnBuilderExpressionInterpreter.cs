using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Borm.Model.Construction;

internal static class ColumnBuilderExpressionInterpreter<TEntity>
    where TEntity : class
{
    public static ColumnBuilder<TEntity> Interpret(
        Expression<Func<ColumnBuilder<TEntity>, TEntity, ColumnBuilder<TEntity>>> expression
    )
    {
        if (expression.Body is not MethodCallExpression call)
        {
            throw new ArgumentException("Expression body must be a method call.");
        }

        return RebuildChain(call, new ColumnBuilder<TEntity>());
    }

    private static object? EvaluateArgumentExpression(Expression expression) =>
        expression switch
        {
            ConstantExpression c => c.Value,
            MemberExpression m => ExtractMemberName(m),
            _ => null,
        };

    private static string? ExtractMemberName(MemberExpression m) =>
        m.Member is PropertyInfo property ? property.Name : null;

    private static ColumnBuilder<TEntity> RebuildChain(
        MethodCallExpression call,
        ColumnBuilder<TEntity> builder
    )
    {
        if (call.Object is MethodCallExpression inner)
        {
            builder = RebuildChain(inner, builder);
        }

        ReadOnlyCollection<Expression> arguments = call.Arguments;
        object?[] args = new object?[arguments.Count];
        for (int i = 0; i < args.Length; i++)
        {
            object? arg = EvaluateArgumentExpression(call.Arguments[i]);
            Debug.Assert(arg is string);
            args[i] = arg;
        }

        return (ColumnBuilder<TEntity>)call.Method.Invoke(builder, args)!;
    }
}
