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
            EntityConfigurator<DebugEntity>
                .Builder()
                .Name("debug_infos")
                .Column(b => b.Index(0).Mapping(e => e.Id).PrimaryKey())
                .Column(b => b.Index(1).Mapping(e => e.Begin, "begin_date"))
                .Column(b => b.Index(2).Mapping(e => e.End, "end_date"))
                .Column(b => b.Index(3).Mapping(e => e.Metadata, "meta"))
                .Build(),
            EntityConfigurator<AddressEntity>.FromType(new AddressEntity.Validator()),
            EntityConfigurator<PersonEntity>.FromType(),
            EntityConfigurator<EmployeeEntity>.FromType(),
        ];

        BormConfig config = new BormConfig.Builder().Model(model).InMemory().Build();

        return new DataContext(config);
    }
}
