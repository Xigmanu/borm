using System.Diagnostics;
using System.Reflection;
using Borm.Extensions;

namespace Borm.Schema;

internal sealed class TableNodeFactory
{
    private readonly Type _entityType;

    public TableNodeFactory(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        _entityType = entityType;
    }

    public TableNode Create()
    {
        TableAttribute tableAttribute = _entityType.GetAttributeOrThrow<TableAttribute>();
        Debug.Assert(tableAttribute != null);
        string name = tableAttribute.Name ?? CreateDefaultName(_entityType);

        IEnumerable<ColumnInfo> columns = _entityType
            .GetProperties()
            .Where(property => property.HasAttribute<ColumnAttribute>())
            .Select(CreateColumnInfo)
            .OrderBy(columnInfo => columnInfo.Index);

        ConstructorInfo? ctor = new EntityConstructorResolver(
            columns,
            _entityType
        ).GetAllColumnsConstructor();

        return new TableNode(name, _entityType, columns, ctor);
    }

    private static string CreateDefaultName(MemberInfo memberInfo)
    {
        string typeName = memberInfo.Name;
        char first = typeName[0];
        if (char.IsUpper(first))
        {
            return typeName.Length == 1
                ? char.ToLower(first).ToString()
                : char.ToLower(first) + typeName[1..];
        }
        return typeName;
    }

    private static Type? FindReferencedEntityType(ColumnAttribute columnAttribute)
    {
        return columnAttribute is ForeignKeyAttribute foreignKeyAttribute
            ? foreignKeyAttribute.ReferencedEntityType
            : null;
    }

    private static Constraints GetConstraints(ColumnAttribute columnAttribute)
    {
        Constraints constraints = Constraints.None;
        if (columnAttribute is PrimaryKeyAttribute)
        {
            constraints |= Constraints.PrimaryKey;
        }
        return constraints;
    }

    private ColumnInfo CreateColumnInfo(PropertyInfo propertyInfo)
    {
        ColumnAttribute columnAttribute = propertyInfo.GetAttributeOrThrow<ColumnAttribute>();

        string? columnName = columnAttribute.Name ?? CreateDefaultName(propertyInfo);
        MethodInfo valueGetter =
            propertyInfo.GetGetMethod()
            ?? throw new MissingMethodException(
                $"Property must have a public getter. Type: {_entityType.FullName}, Property: {propertyInfo.Name}"
            );
        MethodInfo? valueSetter = propertyInfo.GetSetMethod();

        Type dataType = propertyInfo.UnwrapNullableType(out bool isNullable);
        Constraints constraints = GetConstraints(columnAttribute);
        if (isNullable)
        {
            constraints |= Constraints.AllowDbNull;
        }
        Type? referencedEntityType = FindReferencedEntityType(columnAttribute);

        return new ColumnInfo(
            columnAttribute.Index,
            columnName,
            dataType,
            valueGetter,
            valueSetter,
            constraints,
            referencedEntityType
        );
    }
}
