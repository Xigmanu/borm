using Borm.Model;

namespace Borm.Tests.Common;

[Entity("employees")]
public sealed class EmployeeEntity
{
    [PrimaryKey(0, "id")]
    public int Id { get; set; }

    [Column(2, "is_active")]
    public bool IsActive { get; set; }

    [ForeignKey(
        1,
        "person_id",
        typeof(PersonEntity),
        IsUnique = true,
        OnDelete = ReferentialAction.Cascade
    )]
    public int Person { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is EmployeeEntity other
            && Id.Equals(other.Id)
            && IsActive.Equals(other.IsActive)
            && Person.Equals(other.Person);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, IsActive, Person);
    }
}
