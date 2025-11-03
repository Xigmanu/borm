using Borm.Model;
using Borm.Model.Construction;
using Borm.Tests.Common;
using Borm.Tests.EndToEnd.Entities;

namespace Borm.Tests.EndToEnd;

internal static class DataContextProvider
{
    public static DataContext CreateDataContext()
    {
        List<EntityInfo> model =
        [
            new EntityBuilder<DebugEntity>()
                .Name("debug_infos")
                .Column(b => b.Index(0).Mapping(e => e.Id).PrimaryKey())
                .Column(b => b.Index(1).Mapping("begin_date", e => e.Begin))
                .Column(b => b.Index(2).Mapping("end_date", e => e.End))
                .Column(b => b.Index(3).Mapping("meta", e => e.Metadata))
                .Build(),
            EntityFactory.FromType(typeof(AddressEntity), new AddressEntity.Validator()),
            EntityFactory.FromType(typeof(PersonEntity)),
            EntityFactory.FromType(typeof(EmployeeEntity)),
        ];

        BormConfig config = new BormConfig.Builder().Model(model).InMemory().Build();

        return new DataContext(config);
    }
}
