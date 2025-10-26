using System.Diagnostics;
using System.Reflection;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Metadata;

internal static class ConstructorSelector
{
    public static Constructor? FindMappingCtor(
        IReadOnlyList<Constructor> constructors,
        HashSet<string> columnNames
    )
    {
        if (constructors.Count == 1 && constructors[0].Parameters.Count == 0)
        {
            return constructors[0];
        }

        for (int i = 0; i < constructors.Count; i++)
        {
            Constructor current = constructors[i];
            if (
                columnNames.Count == current.Parameters.Count
                && IsCtorParamListValid(current.Parameters, columnNames)
            )
            {
                return current;
            }
        }

        return null;
    }

    private static bool IsCtorParamListValid(
        IReadOnlyList<MappingMember> parameters,
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
