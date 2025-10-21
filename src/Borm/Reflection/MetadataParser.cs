using System.Linq.Expressions;
using System.Reflection;
using Borm.Model;
using Borm.Properties;

namespace Borm.Reflection;

internal sealed class MetadataParser
{
    private readonly NullabilityInfoContext _nullabilityCtx;

    public MetadataParser()
    {
        _nullabilityCtx = new();
    }

    public EntityTypeInfo Parse(Type entityType)
    {
        EntityAttribute entityAttribute =
            entityType.GetCustomAttribute<EntityAttribute>()
            ?? throw new MemberAccessException(
                Strings.EntityTypeNotDecorated(entityType.FullName!, nameof(EntityAttribute))
            );

        List<MappingMember> properties = [];
        PropertyInfo[] typeProperties = entityType.GetProperties();
        for (int i = 0; i < typeProperties.Length; i++)
        {
            PropertyInfo propertyInfo = typeProperties[i];
            ColumnAttribute? attribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
            if (attribute != null)
            {
                NullableType type = ParseMemberType(propertyInfo);
                MappingMember property = new(
                    propertyInfo.Name,
                    type,
                    MappingInfo.FromAttribute(attribute)
                );
                properties.Add(property);
            }
        }

        IReadOnlyList<Constructor> constructors = entityType
            .GetConstructors()
            .Select(ParseConstructorInfo)
            .ToList()
            .AsReadOnly();

        return new EntityTypeInfo(
            entityAttribute.Name,
            entityType,
            properties.AsReadOnly(),
            constructors
        );
    }

    private Constructor ParseConstructorInfo(ConstructorInfo ctor)
    {
        List<MappingMember> parsedParams = [];
        ParameterInfo[] parameters = ctor.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo param = parameters[i];
            NullableType type = ParseMemberType(param);
            MappingMember parsedParam = new(param.Name!, type, Mapping: null);
            parsedParams.Add(parsedParam);
        }

        return new Constructor(
            parameters.Length == 0,
            parsedParams.AsReadOnly(),
            (args) => Expression.New(ctor, args)
        );
    }

    private NullableType ParseMemberType(ICustomAttributeProvider member)
    {
        static bool isNull(NullabilityInfo info) => info.ReadState == NullabilityState.Nullable;
        return member switch
        {
            PropertyInfo prop => new NullableType(
                prop.PropertyType,
                isNull(_nullabilityCtx.Create(prop))
            ),
            ParameterInfo param => new NullableType(
                param.ParameterType,
                isNull(_nullabilityCtx.Create(param))
            ),
            _ => throw new NotSupportedException(),
        };
    }
}
