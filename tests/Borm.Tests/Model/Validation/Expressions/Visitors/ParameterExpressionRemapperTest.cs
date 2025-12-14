using System.Linq.Expressions;
using Borm.Model.Validation.Expressions.Visitors;

namespace Borm.Tests.Model.Validation.Expressions.Visitors;

public sealed class ParameterExpressionRemapperTest
{
    [Fact]
    public void Visit_RemapsParametersToProvided()
    {
        // Arrange
        Expression<Func<object, bool>> expression = o => (bool)o;
        Expression body = expression.Body;
        ParameterExpression parameter = Expression.Parameter(typeof(string), "prop");

        ParameterExpressionRemapper remapper = new(expression.Parameters[0], parameter);

        // Act
        Expression remapped = remapper.Visit(body);

        // Assert
        Assert.NotNull(new RecursiveAsserterVisitor(parameter).Visit(remapped));
    }
}