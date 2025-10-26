using Borm.Data.Storage;
using Borm.Model.Metadata;
using Moq;

namespace Borm.Tests.Mocks.Builders;

internal sealed class ValueBufferImplBuilder
{
    private readonly Mock<IValueBuffer> _mock = new();

    public IValueBuffer Build() => _mock.Object;

    public ValueBufferImplBuilder ColumnValues(Dictionary<IColumnMetadata, object> columnValues)
    {
        Dictionary<string, object> colNameValues = columnValues.ToDictionary(
            kvp => kvp.Key.Name,
            kvp => kvp.Value
        );
        _mock
            .Setup(v => v.GetEnumerator())
            .Returns(() => columnValues.GetEnumerator());
        MockIndexers(columnValues, colNameValues);
        MockCopy(columnValues);

        return this;
    }

    public ValueBufferImplBuilder PrimaryKey(object primaryKey)
    {
        _mock.SetupGet(v => v.PrimaryKey).Returns(primaryKey);
        return this;
    }

    private void MockCopy(Dictionary<IColumnMetadata, object> columnValues)
    {
        _mock
            .Setup(v => v.Copy())
            .Returns(() =>
            {
                Dictionary<IColumnMetadata, object> copy = new(columnValues);
                return new ValueBufferImplBuilder()
                    .ColumnValues(copy)
                    .PrimaryKey(
                        columnValues
                            .First(kvp =>
                                kvp.Key.Constraints.HasFlag(Borm.Model.Constraints.PrimaryKey)
                            )
                            .Value
                    )
                    .Build();
            });
    }

    private void MockIndexers(
        Dictionary<IColumnMetadata, object> columnValues,
        Dictionary<string, object> colNameValues
    )
    {
        _mock
            .Setup(v => v[It.IsAny<IColumnMetadata>()])
            .Returns<IColumnMetadata>(c => columnValues[c]);
        _mock.Setup(v => v[It.IsAny<string>()]).Returns<string>(c => colNameValues[c]);
        _mock
            .SetupSet(v => v[It.IsAny<IColumnMetadata>()] = It.IsAny<object>())
            .Callback<IColumnMetadata, object>(
                (column, value) =>
                {
                    columnValues[column] = value;
                    colNameValues[column.Name] = value;
                }
            );
    }
}
