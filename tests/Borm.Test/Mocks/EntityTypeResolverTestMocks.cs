using Borm.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Tests.Mocks;

[ExcludeFromCodeCoverage]
internal static class EntityTypeResolverTestMocks
{
    [Entity]
    public sealed class MockEntity;

    [Entity]
    public abstract class AbstractMockEntity;
}
