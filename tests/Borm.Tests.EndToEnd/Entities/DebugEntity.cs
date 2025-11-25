namespace Borm.Tests.EndToEnd.Entities;

public sealed class DebugEntity
{
    public DateTime Begin { get; set; }
    public DateTime End { get; set; }
    public Guid Id { get; set; }

    public string? Metadata { get; set; }
}