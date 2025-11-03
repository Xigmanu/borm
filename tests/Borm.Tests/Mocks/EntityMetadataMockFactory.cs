using Borm.Model.Conversion;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks.Builders;

namespace Borm.Tests.Mocks;

internal static class EntityMetadataMockFactory
{
    public static IEntityMetadata CreateMockAddressEntity()
    {
        List<IColumnMetadata> columns =
        [
            new ColumnMetadataImplBuilder()
                .Index(0)
                .Name("id")
                .PropertyName("Id")
                .DataType(typeof(int), isNullable: false)
                .PrimaryKey()
                .Build(),
            new ColumnMetadataImplBuilder()
                .Index(1)
                .Name("address")
                .PropertyName("Address")
                .DataType(typeof(string), isNullable: false)
                .Build(),
            new ColumnMetadataImplBuilder()
                .Index(2)
                .Name("address_1")
                .PropertyName("Address_1")
                .DataType(typeof(string), isNullable: true)
                .Nullable()
                .Build(),
            new ColumnMetadataImplBuilder()
                .Index(3)
                .Name("city")
                .PropertyName("City")
                .DataType(typeof(string), isNullable: false)
                .Unique()
                .Build(),
        ];

        IEntityBufferConversion conversion = new EntityBufferConversionImplBuilder()
            .MaterializeEntity(buffer =>
            {
                return new AddressEntity(
                    (int)buffer["id"],
                    (string)buffer["address"],
                    (string?)(buffer["address_1"] != DBNull.Value ? buffer["address_1"] : null),
                    (string)buffer["city"]
                );
            })
            .ToValueBuffer(entity =>
            {
                AddressEntity address = (AddressEntity)entity;
                Dictionary<IColumnMetadata, object> columnValues = [];
                columnValues[columns[0]] = address.Id;
                columnValues[columns[1]] = address.Address;
                columnValues[columns[2]] =
                    address.Address_1 != null ? address.Address_1 : DBNull.Value;
                columnValues[columns[3]] = address.City;

                return new ValueBufferImplBuilder()
                    .ColumnValues(columnValues)
                    .PrimaryKey(address.Id)
                    .Build();
            })
            .Build();

        return new EntityMetadataImplBuilder()
            .Name("addresses")
            .Type(typeof(AddressEntity))
            .Columns(columns)
            .PrimaryKey(columns[0])
            .Conversion(conversion)
            .Validate(entity =>
            {
                if (string.IsNullOrWhiteSpace(((AddressEntity)entity).Address))
                {
                    throw new InvalidOperationException(
                        "Validation error: Address cannot be null or whitespace"
                    );
                }
            })
            .Build();
    }

    public static IEntityMetadata CreateMockEmployeeEntity()
    {
        List<IColumnMetadata> columns =
        [
            new ColumnMetadataImplBuilder()
                .Index(0)
                .Name("id")
                .PropertyName("Id")
                .DataType(typeof(int), isNullable: false)
                .PrimaryKey()
                .Build(),
            new ColumnMetadataImplBuilder()
                .Index(1)
                .Name("person_id")
                .PropertyName("Person")
                .DataType(typeof(int), isNullable: false)
                .Unique()
                .Reference(typeof(PersonEntity))
                .Build(),
            new ColumnMetadataImplBuilder()
                .Index(2)
                .Name("is_active")
                .PropertyName("IsActive")
                .DataType(typeof(bool), isNullable: false)
                .Build(),
        ];

        IEntityBufferConversion conversion = new EntityBufferConversionImplBuilder()
            .MaterializeEntity(buffer =>
            {
                return new EmployeeEntity()
                {
                    Id = (int)buffer["id"],
                    Person = (int)buffer["person_id"],
                    IsActive = (bool)buffer["is_active"],
                };
            })
            .ToValueBuffer(entity =>
            {
                EmployeeEntity employee = (EmployeeEntity)entity;
                Dictionary<IColumnMetadata, object> columnValues = [];
                columnValues[columns[0]] = employee.Id;
                columnValues[columns[1]] = employee.Person;
                columnValues[columns[2]] = employee.IsActive;

                return new ValueBufferImplBuilder()
                    .ColumnValues(columnValues)
                    .PrimaryKey(employee.Id)
                    .Build();
            })
            .Build();

        return new EntityMetadataImplBuilder()
            .Name("employees")
            .Type(typeof(EmployeeEntity))
            .Columns(columns)
            .PrimaryKey(columns[0])
            .Conversion(conversion)
            .Build();
    }

    public static IEntityMetadata CreateMockPersonEntity()
    {
        List<IColumnMetadata> columns =
        [
            new ColumnMetadataImplBuilder()
                .Index(0)
                .Name("id")
                .PropertyName("Id")
                .DataType(typeof(int), isNullable: false)
                .PrimaryKey()
                .Build(),
            new ColumnMetadataImplBuilder()
                .Index(1)
                .Name("name")
                .PropertyName("Name")
                .DataType(typeof(string), isNullable: false)
                .Unique()
                .Build(),
            new ColumnMetadataImplBuilder()
                .Index(2)
                .Name("salary")
                .PropertyName("Salary")
                .DataType(typeof(double), isNullable: false)
                .Build(),
            new ColumnMetadataImplBuilder()
                .Index(3)
                .Name("address")
                .PropertyName("Address")
                .DataType(typeof(AddressEntity), isNullable: true)
                .Reference(typeof(AddressEntity))
                .Nullable()
                .Build(),
        ];

        IEntityBufferConversion conversion = new EntityBufferConversionImplBuilder()
            .MaterializeEntity(buffer =>
            {
                return new PersonEntity(
                    (int)buffer["id"],
                    (string)buffer["name"],
                    (double)buffer["salary"],
                    buffer["address"] == DBNull.Value ? null : (AddressEntity)buffer["address"]
                );
            })
            .ToValueBuffer(entity =>
            {
                PersonEntity person = (PersonEntity)entity;
                Dictionary<IColumnMetadata, object> columnValues = new();
                columnValues[columns[0]] = person.Id;
                columnValues[columns[1]] = person.Name;
                columnValues[columns[2]] = person.Salary;
                columnValues[columns[3]] = person.Address == null ? DBNull.Value : person.Address;

                return new ValueBufferImplBuilder()
                    .ColumnValues(columnValues)
                    .PrimaryKey(person.Id)
                    .Build();
            })
            .Build();

        return new EntityMetadataImplBuilder()
            .Name("persons")
            .Type(typeof(PersonEntity))
            .Columns(columns)
            .PrimaryKey(columns[0])
            .Conversion(conversion)
            .Build();
    }
}
