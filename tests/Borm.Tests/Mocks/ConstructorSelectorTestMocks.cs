using System.Diagnostics.CodeAnalysis;

namespace Borm.Tests.Mocks;

[ExcludeFromCodeCoverage]
internal static class ConstructorSelectorTestMocks
{
#pragma warning disable S2094, CS9113
    public sealed class DefaultCtorEntity;

    public sealed class InvalidCtorEntity(int id, string foo);

    public sealed class UnEqualParameterCountCtorEntity(int id);

    public sealed class ValidCtorEntity(int id, string name);
#pragma warning restore S2094, CS9113
}
