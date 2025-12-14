using Borm.Model;
using Borm.Model.Validation;

namespace Borm.Tests.Common;

[Entity("addresses")]
[Validator(typeof(Validator))]
public sealed class AddressEntity(int id, string address, string? address_1, string city)
{
    [Column(1, "address")] public string Address { get; } = address;

    [Column(2, "address_1")] public string? Address_1 { get; } = address_1;

    [Column(3, "city")] public string City { get; } = city;

    [PrimaryKey(0)] public int Id { get; } = id;

    public Guid InternalId { get; set; }

    public static AddressEntity CreateFromArray(object[] values)
    {
        return new AddressEntity(
            (int)values[0],
            (string)values[1],
            values[2] == DBNull.Value ? null : (string)values[2],
            (string)values[3]
        );
    }

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

    public sealed class Validator : IObjectValidator<AddressEntity>
    {
        public ValidationResult Validate(AddressEntity entity)
        {
            return string.IsNullOrWhiteSpace(entity.Address)
                ? ValidationResult.Error(entity.Address)
                : ValidationResult.Ok();
        }
    }
}