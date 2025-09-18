using Borm.Model;

namespace Borm.Tests.Common;

[Entity("persons")]
public sealed class PersonEntity(int id, string name, double salary, AddressEntity? address)
{
    [ForeignKey(3, "address", typeof(AddressEntity), OnDelete = ReferentialAction.SetNull)]
    public AddressEntity? Address { get; } = address;

    [PrimaryKey(0)]
    public int Id { get; } = id;

    [Column(1, "name")]
    public string Name { get; } = name;

    [Column(2, "salary")]
    public double Salary { get; } = salary;

    public override bool Equals(object? obj)
    {
        return obj is PersonEntity other
            && Id.Equals(other.Id)
            && Name.Equals(other.Name)
            && Salary.Equals(other.Salary)
            && (
                Address is not null && Address.Equals(other.Address)
                || (other.Address is not null && other.Address.Equals(Address))
                || Address == other.Address
            );
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Salary, Address);
    }
}
