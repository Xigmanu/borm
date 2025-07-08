using System.Diagnostics;
using System.Reflection;

namespace Borm.Schema.Metadata;

internal sealed class EntityConstructorResolver
{
    private readonly ColumnInfoCollection _columns;
    private readonly Type _entityType;

    public EntityConstructorResolver(ColumnInfoCollection columns, Type entityType)
    {
        _columns = columns;
        _entityType = entityType;
    }

    public ConstructorInfo GetAllColumnsConstructor(out bool isImplicitCtor)
    {
        ConstructorInfo[] constructors = _entityType.GetConstructors();
        ConstructorInfo constructor = constructors[0];
        isImplicitCtor =
            constructors.Length == 1
            && constructor.Equals(_entityType.GetConstructor(Type.EmptyTypes));

        if (isImplicitCtor)
        {
            return constructor;
        }

        for (int i = 0; i < constructors.Length; i++)
        {
            ConstructorInfo current = constructors[i];
            List<ParameterInfo> parameters = [.. current.GetParameters()];
            if (_columns.Count == parameters.Count && IsCtorParamListValid(parameters))
            {
                return current;
            }
        }

        throw new MissingMethodException(
            $"Type {_entityType.Name} does not have a public constructor that would initialize all columns"
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
