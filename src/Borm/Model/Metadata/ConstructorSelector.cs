using System.Diagnostics;
using Borm.Reflection;

namespace Borm.Model.Metadata;

internal static class ConstructorSelector
{
    public static IConstructor? FindMappingCtor(
        IReadOnlyList<IConstructor> constructors,
        HashSet<string> columnNames
    )
    {
        if (columnNames.Count == 0)
        {
            throw new ArgumentException("Column name set is empty");
        }

        if (constructors is [{ IsDefault: true }])
        {
            return constructors[0];
        }

        return constructors.FirstOrDefault(current =>
            columnNames.Count == current.Parameters.Count
            && IsCtorParamListValid(current.Parameters, columnNames)
        );
    }

    private static bool IsCtorParamListValid(
        IReadOnlyList<IMappable> parameters,
        HashSet<string> columnNames
    )
    {
        Debug.Assert(columnNames.Count == parameters.Count);

        foreach (string? parameterName in parameters.Select(param => param.MemberName))
        {
            Debug.Assert(parameterName != null);
            if (!columnNames.Contains(parameterName))
            {
                return false;
            }
        }

        return true;
    }
}