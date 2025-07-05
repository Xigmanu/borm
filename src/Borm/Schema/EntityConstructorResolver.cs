using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Borm.Schema;

internal sealed class EntityConstructorResolver
{
    private readonly IEnumerable<ColumnInfo> _columns;
    private readonly Type _entityType;

    public EntityConstructorResolver(IEnumerable<ColumnInfo> columns, Type entityType)
    {
        _columns = columns;
        _entityType = entityType;
    }

    public ConstructorInfo? GetAllColumnsConstructor()
    {
        ConstructorInfo[] constructors = _entityType.GetConstructors();

        if (constructors.Length > 1)
        {
            throw new NotSupportedException(
                $"Constructor overloading is not supported. Type: {_entityType.FullName}"
            );
        }

        ConstructorInfo constructor = constructors[0];
        List<ParameterInfo> parameters = [.. constructor.GetParameters()];
        if (parameters.Count == 0)
        {
            return null;
        }

        if (parameters.Count != _columns.Count())
        {
            throw new MissingMethodException(
                $"Type {_entityType.Name} does not have a public constructor that would initialize all columns"
            );
        }

        if (!IsCtorParamListValid(parameters, out Exception? exception))
        {
            throw exception;
        }

        return constructor;
    }

    private bool IsCtorParamListValid(
        List<ParameterInfo> parameters,
        [NotNullWhen(false)] out Exception? exception
    )
    {
        exception = null;
        HashSet<string> columnNameSet = [.. _columns.Select(column => column.Name)];
        Debug.Assert(columnNameSet.Count == parameters.Count);

        foreach (string? parameterName in parameters.Select(param => param.Name))
        {
            Debug.Assert(parameterName != null);
            if (!columnNameSet.Contains(parameterName))
            {
                exception = new MissingMethodException(
                    $"Type {_entityType.Name} does not have a public constructor that would initialize all columns"
                );
                return false;
            }
        }

        return true;
    }
}
