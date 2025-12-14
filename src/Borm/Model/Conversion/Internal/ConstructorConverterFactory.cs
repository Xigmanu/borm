using System.Linq.Expressions;
using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Model.Conversion.Internal;

internal sealed class ConstructorConverterFactory : ConverterFactory<Func<IValueBuffer, object>>
{
    private readonly IConstructor _constructor;

    public ConstructorConverterFactory(
        IConstructor constructor,
        IReadOnlyList<IColumnMetadata> columns
    )
        : base(columns)
    {
        if (constructor.IsDefault)
        {
            throw new ArgumentException("Cannot use a default constructor for conversion");
        }

        _constructor = constructor;
    }

    public override Func<IValueBuffer, object> Create()
    {
        ParameterExpression bufferParam = Expression.Parameter(typeof(IValueBuffer), "buffer");
        IEnumerable<Expression> args = GetOrderedColumns(_constructor.Parameters)
            .Select(col => CreateBufferPropertyBinding(bufferParam, col));
        Expression ctorCall = _constructor.CreateNewInstanceExpression(args);

        return Expression
            .Lambda<Func<IValueBuffer, object>>(
                Expression.Convert(ctorCall, typeof(object)),
                bufferParam
            )
            .Compile();
    }

    private IEnumerable<IColumnMetadata> GetOrderedColumns(IReadOnlyList<IMappable> ctorParams)
    {
        Dictionary<string, IColumnMetadata> colNames = Columns.ToDictionary(col => col.Name);
        return ctorParams.Select(param => colNames[param.MemberName]);
    }
}