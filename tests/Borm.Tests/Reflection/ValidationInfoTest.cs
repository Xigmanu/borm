using System.Linq.Expressions;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Reflection;

public sealed class ValidationInfoTest
{
    [Fact]
    public void AdjustToCommonParameter_RemapsInputExpressionToProvidedParameter()
    {
        // Arrange
        ValidationInfo validation = GetExpression();
        ParameterExpression parameter = Expression.Parameter(typeof(AddressEntity), "e");
        RecursiveAsserterVisitor asserter = new(parameter);

        // Act
        (Expression body, Expression propAccess) = validation.AdjustToCommonParameter(parameter);

        // Assert
        Assert.NotNull(asserter.Visit(body));
        Assert.NotNull(asserter.Visit(propAccess));
    }

    private static ValidationInfo GetExpression()
    {
        Expression<Func<AddressEntity, bool>> lambda = address => address.Address.Length == 5;
        MemberExpression propAccess = Expression.Property(
            Expression.Parameter(typeof(AddressEntity), "address"),
            typeof(AddressEntity),
            nameof(AddressEntity.Address)
        );

        return new ValidationInfo(lambda, propAccess);
    }

    private sealed class RecursiveAsserterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _expected;

        public RecursiveAsserterVisitor(ParameterExpression expected)
        {
            _expected = expected;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Assert.Equal(_expected.Name, node.Name);

            return base.VisitParameter(node);
        }
    }
}