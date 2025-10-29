using System.Linq.Expressions;
using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Model.Conversion;

internal sealed class ConstructorConverterFactory : ConverterFactory<Func<IValueBuffer, object>>
{
    private readonly Constructor _constructor;

    public ConstructorConverterFactory(Constructor constructor, IEnumerable<IColumnMetadata> columns)
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

    private IEnumerable<IColumnMetadata> GetOrderedColumns(IReadOnlyList<MappingMember> ctorParams)
    {
        Dictionary<string, IColumnMetadata> colNames = columns.ToDictionary(col => col.Name);
        return ctorParams.Select(param => colNames[param.MemberName]);
    }
}
