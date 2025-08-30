using Borm.Model;

namespace Borm.Tests.Common;

[Entity("addresses")]
public sealed class AddressEntity(int id, string address, string? address_1, string city)
{
    [Column(1, "address")]
    public string Address { get; } = address;

    [Column(2, "address_1")]
    public string? Address_1 { get; } = address_1;

    [Column(3, "city")]
    public string City { get; } = city;

    [PrimaryKey(0)]
    public int Id { get; } = id;

    public override bool Equals(object? obj)
    {
        return obj is AddressEntity other
            && Id.Equals(other.Id)
            && Address.Equals(other.Address)
            && Address_1 == other.Address_1
            && City.Equals(other.City);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Address, Address_1, City);
    }

    public sealed class Validator : IEntityValidator<AddressEntity>
    {
        public void Validate(AddressEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Address))
            {
                throw new InvalidOperationException();
            }
        }
    }
}
