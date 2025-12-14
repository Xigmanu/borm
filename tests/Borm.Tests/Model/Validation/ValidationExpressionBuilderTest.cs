using System.Linq.Expressions;
using Borm.Model.Metadata;
using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;
using Borm.Reflection;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Model.Validation;

public sealed class ValidationExpressionBuilderTest
{
    private readonly ColumnValidatorFactoryContext _context = new();

    [Fact]
    public void TryBuild_ReturnsFalseAndNullValidation_WhenNoValidationDefinedOnProperty()
    {
        // Arrange
        IMappable property = new TestProperty(
            "Id",
            new NullableType(typeof(int), false),
            null,
            null
        );
        ParameterExpression entity = Expression.Parameter(typeof(AddressEntity), "e");
        ParameterExpression metadata = Expression.Parameter(typeof(IEntityMetadata), "meta");
        ParameterExpression result = Expression.Parameter(typeof(ValidationResult), "res");

        ValidationExpressionBuilder builder = new(_context);

        // Act
        bool created = builder.TryBuild(
            property,
            entity,
            metadata,
            result,
            out Expression? validation
        );

        // Assert
        Assert.False(created);
        Assert.Null(validation);
    }

    [Fact]
    public void TryBuild_ReturnsTrueAndValidation_WhenValidationIsDefinedOnProperty()
    {
        // Arrange
        ParameterExpression entity = Expression.Parameter(typeof(AddressEntity), "e");
        ValidationInfo validationInfo = CreateValidationInfo(entity);
        IMappable property = new TestProperty(
            "Id",
            new NullableType(typeof(int), false),
            null,
            validationInfo
        );
        ParameterExpression metadata = Expression.Parameter(typeof(IEntityMetadata), "meta");
        ParameterExpression result = Expression.Parameter(typeof(ValidationResult), "res");

        ValidationExpressionBuilder builder = new(_context);

        // Act
        bool created = builder.TryBuild(
            property,
            entity,
            metadata,
            result,
            out Expression? validation
        );

        // Assert
        Assert.True(created);
        Assert.NotNull(validation);
    }

    private static ValidationInfo CreateValidationInfo(ParameterExpression entity)
    {
        Expression<Func<AddressEntity, bool>> validation = e => e.Id >= 0;
        MemberExpression member = Expression.Property(
            entity,
            typeof(AddressEntity).GetProperty(nameof(AddressEntity.Id))!
        );
        return new ValidationInfo(validation, member);
    }
}