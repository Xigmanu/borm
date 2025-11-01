using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Borm.Model.Construction;

internal static class ColumnBuilderExecutor
{
    public static ColumnBuilder<T> Execute<T>(
        Expression<Func<ColumnBuilder<T>, T, ColumnBuilder<T>>> expression
    )
        where T : class
    {
        if (expression.Body is not MethodCallExpression call)
        {
            throw new ArgumentException("Expression body must be a method call.");
        }

        return RebuildChain(call, new ColumnBuilder<T>());
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

    private static ColumnBuilder<T> RebuildChain<T>(
        MethodCallExpression call,
        ColumnBuilder<T> builder
    )
        where T : class
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

        return (ColumnBuilder<T>)call.Method.Invoke(builder, args)!;
    }
}
