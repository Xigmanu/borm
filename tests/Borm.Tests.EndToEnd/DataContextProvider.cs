using Borm.Model;
using Borm.Model.Construction;
using Borm.Tests.Common;

namespace Borm.Tests.EndToEnd;

internal static class DataContextProvider
{
    public static DataContext CreateDataContext()
    {
        List<EntityInfo> model =
        [
            EntityFactory.FromType(typeof(AddressEntity), new AddressEntity.Validator()),
            EntityFactory.FromType(typeof(PersonEntity)),
            EntityFactory.FromType(typeof(EmployeeEntity)),
        ];

        BormConfig config = new BormConfig.Builder().Model(model).InMemory().Build();

        return new DataContext(config);
    }
}
