using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Mocks;

internal static class EntityInfoMocks
{
    public static readonly EntityMetadata AddressesEntity = new(
        "addresses",
        typeof(AddressEntity),
        new(
            [
                new ColumnMetadata(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null),
                new ColumnMetadata(1, "address", "Address", typeof(string), Constraints.None, null),
                new ColumnMetadata(
                    2,
                    "address_1",
                    "Address_1",
                    typeof(string),
                    Constraints.AllowDbNull,
                    null
                ),
                new ColumnMetadata(3, "city", "City", typeof(string), Constraints.None, null),
            ]
        )
    );

    public static readonly EntityMetadata PersonsEntity = new(
        "persons",
        typeof(PersonEntity),
        new(
            [
                new ColumnMetadata(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null),
                new ColumnMetadata(1, "name", "Name", typeof(string), Constraints.Unique, null),
                new ColumnMetadata(2, "salary", "Salary", typeof(double), Constraints.None, null),
                new ColumnMetadata(
                    3,
                    "address",
                    "Address",
                    typeof(AddressEntity),
                    Constraints.AllowDbNull,
                    typeof(AddressEntity)
                ),
            ]
        )
    );

    [Entity("addresses")]
    internal sealed class AddressEntity(int id, string address, string? address_1, string city)
    {
        [PrimaryKey(0)]
        public int Id { get; } = id;

        [Column(1, "address")]
        public string Address { get; } = address;

        [Column(2, "address_1")]
        public string? Address_1 { get; } = address_1;

        [Column(3, "city")]
        public string City { get; } = city;
    }

    [Entity("persons")]
    internal sealed class PersonEntity(int id, string name, double salary, AddressEntity? address)
    {
        [PrimaryKey(0)]
        public int Id { get; } = id;

        [Column(1, "name")]
        public string Name { get; } = name;

        [Column(2, "salary")]
        public double Salary { get; } = salary;

        [ForeignKey(3, "address", typeof(AddressEntity))]
        public AddressEntity? Address { get; } = address;
    }
}
