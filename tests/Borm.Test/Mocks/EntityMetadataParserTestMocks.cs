using Borm.Schema;

namespace Borm.Tests.Mocks;

internal class EntityMetadataParserTestMocks
{
    [Entity("entities")]
    public sealed class ValidEntity
    {
        [PrimaryKey(0)]
        public int Id { get; }
        [Column(1, "entity_name")]
        public string? Name { get; }
        public bool Exists { get; }
    }
}
