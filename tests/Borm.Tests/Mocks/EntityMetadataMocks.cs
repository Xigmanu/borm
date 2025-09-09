using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Tests.Common;

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
                new ColumnMetadata(0, "id", "Id", typeof(int), Constraints.PrimaryKey),
                new ColumnMetadata(1, "address", "Address", typeof(string), Constraints.None),
                new ColumnMetadata(
                    2,
                    "address_1",
                    "Address_1",
                    typeof(string),
                    Constraints.AllowDbNull
                ),
                new ColumnMetadata(3, "city", "City", typeof(string), Constraints.Unique),
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
        metadata.Validator = (entity) =>
        {
            if (string.IsNullOrWhiteSpace(((AddressEntity)entity).Address))
            {
                throw new InvalidOperationException(name);
            }
        };

        return metadata;
    }

    private static EntityMetadata CreateEmployeeEntity()
    {
        string name = "employees";
        Type dataType = typeof(EmployeeEntity);
        ColumnMetadataCollection columns = new(
            [
                new ColumnMetadata(0, "id", "Id", typeof(int), Constraints.PrimaryKey),
                new ColumnMetadata(1, "person_id", "Person", typeof(int), Constraints.Unique)
                {
                    Reference = typeof(AddressEntity),
                },
                new ColumnMetadata(2, "is_active", "IsActive", typeof(bool), Constraints.None),
            ]
        );

        EntityMetadata metadata = new(name, dataType, columns);
        EntityConversionBinding binding = new(
            (buffer) =>
            {
                return new EmployeeEntity()
                {
                    Id = (int)buffer["id"],
                    Person = (int)buffer["person_id"],
                    IsActive = (bool)buffer["is_active"],
                };
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
                new ColumnMetadata(0, "id", "Id", typeof(int), Constraints.PrimaryKey),
                new ColumnMetadata(1, "name", "Name", typeof(string), Constraints.Unique),
                new ColumnMetadata(2, "salary", "Salary", typeof(double), Constraints.None),
                new ColumnMetadata(
                    3,
                    "address",
                    "Address",
                    typeof(AddressEntity),
                    Constraints.AllowDbNull
                )
                {
                    Reference = typeof(AddressEntity),
                },
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
                buffer[columns["address"]] = person.Address == null ? DBNull.Value : person.Address;
                return buffer;
            }
        );
        metadata.Binding = binding;
        return metadata;
    }
}
