using Borm.Data.Sql;
using Borm.Data.Storage.Tracking;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

internal interface ITable
{
    IEntityMetadata Metadata { get; }
    string Name { get; }
    ChangeTracker Tracker { get; }

    void Delete(IValueBuffer record, long txId);
    void Insert(IValueBuffer record, long txId);
    void Load(ResultSet resultSet, long txId);

    void Update(IValueBuffer record, long txId);
}