using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Borm.Model;
using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;
using Borm.Reflection;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Model.Validation.Expressions;

public sealed class ColumnValidatorFactoryTest
{
    private readonly Type _entityType = typeof(AddressEntity);
    private readonly ColumnValidatorFactoryContext _factoryContext = new();

    [Fact]
    public void Create_ReturnsCompiledDelegate_WhenValidationExistsOnProperties()
    {
        // Arrange
        List<IMappable> properties = CreateBaseAddressEntityMockProperties();
        properties.Add(
            new TestProperty(
                "City",
                new NullableType(typeof(string), false),
                new MappingInfo(3, "city", false, false, null, ReferentialAction.NoAction),
                new ValidationInfo(Expression.Empty(), Expression.Empty())
            )
        );
        ColumnValidatorFactory factory = new(
            _factoryContext,
            new TestValidationExpressionBuilder(),
            _entityType,
            properties
        );

        // Act
        ObjectValidator? validator = factory.Create();

        // Assert
        Assert.NotNull(validator);
    }

    [Fact]
    public void Create_ReturnsNull_WhenNoValidationExistsOnProperties()
    {
        // Arrange
        List<IMappable> properties = CreateBaseAddressEntityMockProperties();
        properties.Add(
            new TestProperty(
                "City",
                new NullableType(typeof(string), false),
                new MappingInfo(3, "city", false, false, null, ReferentialAction.NoAction)
            )
        );

        ColumnValidatorFactory factory = new(
            _factoryContext,
            new TestValidationExpressionBuilder(),
            _entityType,
            properties
        );

        // Act
        ObjectValidator? validator = factory.Create();

        // Assert
        Assert.Null(validator);
    }

    private static List<IMappable> CreateBaseAddressEntityMockProperties() =>
    [
        new TestProperty(
            "Id",
            new NullableType(typeof(Guid), false),
            new MappingInfo(0, "id", true, false, null, ReferentialAction.NoAction)
        ),
        new TestProperty(
            "Address",
            new NullableType(typeof(string), false),
            new MappingInfo(1, "address", false, false, null, ReferentialAction.NoAction)
        ),
        new TestProperty(
            "Address_1",
            new NullableType(typeof(string), true),
            new MappingInfo(2, "address_1", false, false, null, ReferentialAction.NoAction)
        )
    ];

    private sealed class TestValidationExpressionBuilder : IValidationExpressionBuilder
    {
        public bool TryBuild(
            IMappable property,
            ParameterExpression entity,
            ParameterExpression metadata,
            ParameterExpression result,
            [NotNullWhen(true)] out Expression? validation
        )
        {
            validation = null;
            if (property.ValidationInfo == null)
            {
                return false;
            }

            validation = Expression.Empty();
            return true;
        }
    }
}