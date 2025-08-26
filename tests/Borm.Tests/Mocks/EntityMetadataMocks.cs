using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Mocks;

internal static class EntityMetadataMocks
{
    public static readonly EntityMetadata AddressesEntity = CreateAddressesEntity();
    public static readonly EntityMetadata EmployeesEntity = CreateEmployeeEntity();
    public static readonly EntityMetadata PersonsEntity = CreatePersonsEntity();

    private static EntityMetadata CreateAddressesEntity()
    {
        string name = "addresses";
        Type dataType = typeof(AddressEntity);
        ColumnMetadataCollection columns = new(
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
                new ColumnMetadata(3, "city", "City", typeof(string), Constraints.Unique, null),
            ]
        );

        EntityMetadata metadata = new(name, dataType, columns);
        EntityConversionBinding binding = new(
            (buffer) =>
            {
                return new AddressEntity(
                    (int)buffer["id"],
                    (string)buffer["address"],
                    (string?)(buffer["address_1"] != DBNull.Value ? buffer["address_1"] : null),
                    (string)buffer["city"]
                );
            },
            (entity) =>
            {
                AddressEntity address = (AddressEntity)entity;
                ValueBuffer buffer = new();
                buffer[columns["id"]] = address.Id;
                buffer[columns["address"]] = address.Address;
                buffer[columns["address_1"]] =
                    address.Address_1 == null ? DBNull.Value : address.Address_1;
                buffer[columns["city"]] = address.City;
                return buffer;
            }
        );
        metadata.Binding = binding;

        return metadata;
    }

    private static EntityMetadata CreateEmployeeEntity()
    {
        string name = "employees";
        Type dataType = typeof(EmployeeEntity);
        ColumnMetadataCollection columns = new(
            [
                new ColumnMetadata(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null),
                new ColumnMetadata(
                    1,
                    "person_id",
                    "Person",
                    typeof(int),
                    Constraints.Unique,
                    typeof(AddressEntity)
                ),
                new ColumnMetadata(
                    2,
                    "is_active",
                    "IsActive",
                    typeof(bool),
                    Constraints.None,
                    null
                ),
            ]
        );

        EntityMetadata metadata = new(name, dataType, columns);
        EntityConversionBinding binding = new(
            (buffer) =>
            {
                return new EmployeeEntity(
                    (int)buffer["id"],
                    (int)buffer["person_id"],
                    (bool)buffer["is_active"]
                );
            },
            (entity) =>
            {
                EmployeeEntity employee = (EmployeeEntity)entity;
                ValueBuffer buffer = new();
                buffer[columns["id"]] = employee.Id;
                buffer[columns["person_id"]] = employee.Person;
                buffer[columns["is_active"]] = employee.IsActive;
                return buffer;
            }
        );
        metadata.Binding = binding;

        return metadata;
    }

    private static EntityMetadata CreatePersonsEntity()
    {
        string name = "persons";
        Type dataType = typeof(PersonEntity);
        ColumnMetadataCollection columns = new(
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
        );

        EntityMetadata metadata = new(name, dataType, columns);
        EntityConversionBinding binding = new(
            (buffer) =>
            {
                return new PersonEntity(
                    (int)buffer["id"],
                    (string)buffer["name"],
                    (double)buffer["salary"],
                    buffer["address"] == DBNull.Value ? null : (AddressEntity)buffer["address"]
                );
            },
            (entity) =>
            {
                PersonEntity person = (PersonEntity)entity;
                ValueBuffer buffer = new();
                buffer[columns["id"]] = person.Id;
                buffer[columns["name"]] = person.Name;
                buffer[columns["salary"]] = person.Salary;
                buffer[columns["address"]] =
                    person.Address == null ? DBNull.Value : person.Address.Id;
                return buffer;
            }
        );
        metadata.Binding = binding;
        return metadata;
    }

    [Entity("addresses")]
    internal sealed class AddressEntity(int id, string address, string? address_1, string city)
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
    }

    [Entity("employees")]
    internal sealed class EmployeeEntity(int id, int person_id, bool is_active)
    {
        [PrimaryKey(0, "id")]
        public int Id { get; } = id;

        [Column(2, "is_active")]
        public bool IsActive { get; } = is_active;

        [ForeignKey(1, "person_id", typeof(PersonEntity), IsUnique = true)]
        public int Person { get; } = person_id;
    }

    [Entity("persons")]
    internal sealed class PersonEntity(int id, string name, double salary, AddressEntity? address)
    {
        [ForeignKey(3, "address", typeof(AddressEntity))]
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
}
