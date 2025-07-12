using System.Diagnostics;
using System.Reflection;

namespace Borm.Schema.Metadata;

internal sealed class EntityConstructorSelector
{
    private readonly ColumnInfoCollection _columns;
    private readonly ConstructorInfo[] _entityConstructors;

    public EntityConstructorSelector(
        ColumnInfoCollection columns,
        ConstructorInfo[] entityConstructors
    )
    {
        _columns = columns;
        _entityConstructors = entityConstructors;
    }

    public ConstructorInfo? Select()
    {
        if (_entityConstructors[0].GetParameters().Length == 0)
        {
            return null;
        }

        for (int i = 0; i < _entityConstructors.Length; i++)
        {
            ConstructorInfo current = _entityConstructors[i];
            List<ParameterInfo> parameters = [.. current.GetParameters()];
            if (_columns.Count == parameters.Count && IsCtorParamListValid(parameters))
            {
                return current;
            }
        }

        throw new MissingMethodException(
            $"Type {_entityConstructors[0].DeclaringType!.FullName} does not have a public constructor that would initialize all columns"
        );
    }

    private bool IsCtorParamListValid(List<ParameterInfo> parameters)
    {
        HashSet<string> columnNameSet = [.. _columns.Select(column => column.Name)];
        Debug.Assert(columnNameSet.Count == parameters.Count);

        foreach (string? parameterName in parameters.Select(param => param.Name))
        {
            Debug.Assert(parameterName != null);
            if (!columnNameSet.Contains(parameterName))
            {
                return false;
            }
        }

        return true;
    }
}
