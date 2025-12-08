using Borm.Model.Validation.Expressions;

namespace Borm.Tests.Model.Validation.Expressions;

public sealed class ColumnValidatorFactoryContextTest
{
    [Fact]
    public void Constructor_InitializesNewInstanceWithAllPropertiesNotNull()
    {
        // Act
        ColumnValidatorFactoryContext context = new();

        // Assert
        Assert.NotNull(context.OkMethod);
        Assert.NotNull(context.CMetaName);
        Assert.NotNull(context.EMetaGetColumn);
        Assert.NotNull(context.EMetaName);
        Assert.NotNull(context.ErrorMethod);
        Assert.NotNull(context.VResultType);
    }
}