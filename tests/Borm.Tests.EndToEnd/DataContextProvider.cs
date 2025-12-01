using Borm.Model.Construction;
using Borm.Tests.Common;
using Borm.Tests.EndToEnd.Entities;

namespace Borm.Tests.EndToEnd;

internal static class DataContextProvider
{
    public static DataContext CreateDataContext()
    {
        ModelConfigurator modelConfigurator = new();
        modelConfigurator.FromBuilder<DebugEntity>(eb =>
            eb.Name("debug_infos")
                .Column(b => b.Index(0).Mapping(e => e.Id).PrimaryKey())
                .Column(b => b.Index(1).Mapping(e => e.Begin, "begin_date"))
                .Column(b => b.Index(2).Mapping(e => e.End, "end_date"))
                .Column(b =>
                    b.Index(3)
                        .Mapping(e => e.Metadata, "meta")
                        .ValidWhen(e => e.Metadata!.StartsWith("m__"))
                )
        );
        modelConfigurator.FromType<AddressEntity>();
        modelConfigurator.FromType<PersonEntity>();
        modelConfigurator.FromType<EmployeeEntity>();

        BormConfig config = new BormConfig.Builder().Model(modelConfigurator).InMemory().Build();

        return new DataContext(config);
    }
}