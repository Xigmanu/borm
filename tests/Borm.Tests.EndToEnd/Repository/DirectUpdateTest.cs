using Borm.Data;
using Borm.Data.Storage;
using Borm.Properties;
using Borm.Tests.Common;

namespace Borm.Tests.EndToEnd.Repository;

public sealed class DirectUpdateTest
{
    [Fact]
    public void InvalidSimpleEntity()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        AddressEntity newAddress = new(1, string.Empty, "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(address);
        Exception? exception = Record.Exception(() => repository.Update(newAddress));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<InvalidOperationException>(inner);
        Assert.Equal(Strings.EntityValidationFailed(typeof(AddressEntity)), inner.Message);

        IEnumerable<AddressEntity> addresses = repository.Select();
        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());
    }

    [Fact]
    public void NullEntity()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        Exception? exception = Record.Exception(() => repository.Update(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<ArgumentNullException>(inner);
    }

    [Fact]
    public void ValidComplexRelationalEntity_WithForeignKeyCollision()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        PersonEntity person = new(1, "name", 42.619, address);
        PersonEntity newPerson = new(1, "foo", 42.619, address);
        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();

        // Act
        addressRepo.Insert(address);
        personRepo.Insert(person);

        personRepo.Update(newPerson);

        // Assert
        IEnumerable<AddressEntity> addresses = addressRepo.Select();

        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());

        IEnumerable<PersonEntity> persons = personRepo.Select();
        Assert.Single(persons);
        Assert.Equal(newPerson, persons.First());
    }

    [Fact]
    public void ValidComplexRelationalEntity_WithoutForeignKeyCollision()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        PersonEntity person = new(1, "name", 42.619, address);
        PersonEntity newPerson = new(
            1,
            "foo",
            42.619,
            new AddressEntity(2, "address", null, "city")
        );
        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();

        // Act
        personRepo.Insert(person);
        context.SaveChanges();

        Exception? exception = Record.Exception(() => personRepo.Update(newPerson));

        // Assert
        IEnumerable<AddressEntity> addresses = addressRepo.Select();

        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());

        IEnumerable<PersonEntity> persons = personRepo.Select();
        Assert.Single(persons);
        Assert.Equal(person, persons.First());

        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.IsType<RecordNotFoundException>(inner);
        Assert.Equal(Strings.RowNotFound("addresses", 2), inner.Message);
    }

    [Fact]
    public void ValidSimpleEntity_WithoutPrimaryKeyCollision()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        AddressEntity newAddress = new(2, "address", null, "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(address);
        Exception? exception = Record.Exception(() => repository.Update(newAddress));

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());

        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<RecordNotFoundException>(inner);
        Assert.Equal(Strings.RowNotFound("addresses", newAddress.Id), inner.Message);
    }

    [Fact]
    public void ValidSimpleEntity_WithoutSavingChanges()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        AddressEntity newAddress = new(1, "address", null, "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(address);
        repository.Update(newAddress);

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Single(addresses);
        Assert.Equal(newAddress, addresses.First());
    }

    [Fact]
    public void ValidSimpleEntity_WithSavingChanges()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        AddressEntity newAddress = new(1, "address", null, "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(address);
        context.SaveChanges();
        repository.Update(newAddress);

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Single(addresses);
        Assert.Equal(newAddress, addresses.First());
    }

    [Fact]
    public void ValidSimpleRelationalEntity()
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
        EmployeeEntity newEmployee = new()
        {
            Id = 1,
            Person = person.Id,
            IsActive = false,
        };
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();
        IEntityRepository<EmployeeEntity> employeeRepo = context.GetRepository<EmployeeEntity>();

        // Act
        personRepo.Insert(person);
        employeeRepo.Insert(employee);
        context.SaveChanges();

        employeeRepo.Update(newEmployee);

        // Assert
        IEnumerable<EmployeeEntity> employees = employeeRepo.Select();

        Assert.Single(employees);
        Assert.Equal(newEmployee, employees.First());
    }

    [Fact]
    public void ValidSimpleRelationalEntity_WithNoPrimaryKeyCollision()
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
        EmployeeEntity newEmployee = new()
        {
            Id = 2,
            Person = person.Id,
            IsActive = false,
        };
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();
        IEntityRepository<EmployeeEntity> employeeRepo = context.GetRepository<EmployeeEntity>();

        // Act
        personRepo.Insert(person);
        employeeRepo.Insert(employee);
        context.SaveChanges();

        Exception? exception = Record.Exception(() => employeeRepo.Update(newEmployee));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<RecordNotFoundException>(inner);
        Assert.Equal(Strings.RowNotFound("employees", newEmployee.Id), inner.Message);

        IEnumerable<EmployeeEntity> employees = employeeRepo.Select();

        Assert.Single(employees);
    }
}
