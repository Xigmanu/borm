using System.Data;
using Borm.Data;
using Borm.Data.Storage;
using Borm.Properties;
using Borm.Tests.Common;

namespace Borm.Tests.EndToEnd.Repository;

public sealed class DirectInsertTest
{
    [Fact]
    public void Direct_InsertInvalidSimpleEntity()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity entity = new(1, string.Empty, "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        Exception? exception = Record.Exception(() => repository.Insert(entity));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<InvalidOperationException>(inner);
        Assert.Equal(Strings.EntityValidationFailed(typeof(AddressEntity)), inner.Message);

        IEnumerable<AddressEntity> addresses = repository.Select();
        Assert.Empty(addresses);
    }

    [Fact]
    public void Direct_InsertValidComplexRelationalEntity_WithForeignKeyCollision()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        PersonEntity person = new(1, "name", 42.619, address);
        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();

        // Act
        addressRepo.Insert(address);
        personRepo.Insert(person);

        // Assert
        IEnumerable<AddressEntity> addresses = addressRepo.Select();

        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());

        IEnumerable<PersonEntity> persons = personRepo.Select();
        Assert.Single(persons);
        Assert.Equal(person, persons.First());
    }

    [Fact]
    public void Direct_InsertValidComplexRelationalEntity_WithoutForeignKeyCollision()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        PersonEntity person = new(1, "name", 42.619, address);
        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();

        // Act
        personRepo.Insert(person);

        // Assert
        IEnumerable<AddressEntity> addresses = addressRepo.Select();

        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());

        IEnumerable<PersonEntity> persons = personRepo.Select();
        Assert.Single(persons);
        Assert.Equal(person, persons.First());
    }

    [Fact]
    public void Direct_InsertValidComplexRelationalEntity_WithoutForeignKeyCollision_WithInvalidDependency()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, string.Empty, "address2", "city");
        PersonEntity person = new(1, "name", 42.619, address);
        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();

        // Act
        Exception? exception = Record.Exception(() => personRepo.Insert(person));

        // Assert
        IEnumerable<AddressEntity> addresses = addressRepo.Select();
        IEnumerable<PersonEntity> persons = personRepo.Select();

        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<InvalidOperationException>(inner);
        Assert.Equal(Strings.EntityValidationFailed(typeof(AddressEntity)), inner.Message);

        Assert.Empty(addresses);
        Assert.Empty(persons);
    }

    [Fact]
    public void Direct_InsertValidSimpleEntity_WithoutSavingChanges()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity entity = new(1, "address", "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(entity);

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Single(addresses);
        Assert.Equal(entity, addresses.First());
    }

    [Fact]
    public void Direct_InsertValidSimpleEntity_WithPrimaryKeyCollision_WithoutSavingChanges()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity entity = new(1, "address", "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(entity);
        Exception? exception = Record.Exception(() => repository.Insert(entity));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<ConstraintException>(inner);
        Assert.Equal(Strings.PrimaryKeyConstraintViolation("addresses", entity.Id), inner.Message);

        IEnumerable<AddressEntity> addresses = repository.Select();
        Assert.Single(addresses);
    }

    [Fact]
    public void Direct_InsertValidSimpleEntity_WithPrimaryKeyCollision_WithSavingChanges()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity entity = new(1, "address", "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(entity);
        context.SaveChanges();
        Exception? exception = Record.Exception(() => repository.Insert(entity));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<ConstraintException>(inner);
        Assert.Equal(Strings.PrimaryKeyConstraintViolation("addresses", entity.Id), inner.Message);

        IEnumerable<AddressEntity> addresses = repository.Select();
        Assert.Single(addresses);
    }

    [Fact]
    public void Direct_InsertValidSimpleEntity_WithSavingChanges()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity entity = new(1, "address", "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(entity);
        context.SaveChanges();

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Single(addresses);
        Assert.Equal(entity, addresses.First());
    }

    [Fact]
    public void Direct_InsertValidSimpleRelationalEntity()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        PersonEntity person = new(1, "name", 42.619, null);
        EmployeeEntity employee = new()
        {
            Id = 1,
            Person = person.Id,
            IsActive = true,
        };
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();
        IEntityRepository<EmployeeEntity> employeeRepo = context.GetRepository<EmployeeEntity>();

        // Act
        personRepo.Insert(person);
        employeeRepo.Insert(employee);

        // Assert
        IEnumerable<EmployeeEntity> employees = employeeRepo.Select();

        Assert.Single(employees);
        Assert.Equal(employee, employees.First());
    }

    [Fact]
    public void Direct_InsertValidSimpleRelationalEntity_WithNoPrimaryKeyCollision()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        PersonEntity person = new(1, "name", 42.619, null);
        EmployeeEntity employee = new()
        {
            Id = 1,
            Person = person.Id,
            IsActive = true,
        };
        IEntityRepository<EmployeeEntity> employeeRepo = context.GetRepository<EmployeeEntity>();

        // Act
        Exception? exception = Record.Exception(() => employeeRepo.Insert(employee));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<RowNotFoundException>(inner);
        Assert.Equal(Strings.RowNotFound("persons", employee.Person), inner.Message);

        IEnumerable<EmployeeEntity> employees = employeeRepo.Select();

        Assert.Empty(employees);
    }
}
