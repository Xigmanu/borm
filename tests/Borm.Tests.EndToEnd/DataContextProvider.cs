using Borm.Model;
using Borm.Tests.Common;

namespace Borm.Tests.EndToEnd;

internal static class DataContextProvider
{
    public static DataContext CreateDataContext()
    {
        EntityModel model = new();
        model.AddEntity(typeof(AddressEntity), new AddressEntity.Validator());
        model.AddEntity(typeof(PersonEntity));
        model.AddEntity(typeof(EmployeeEntity));

        BormConfig config = new BormConfig.Builder().Model(model).InMemory().Build();

        return new DataContext(config);
    }
}
