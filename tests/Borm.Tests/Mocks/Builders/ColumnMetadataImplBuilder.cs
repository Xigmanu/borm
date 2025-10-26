using Borm.Model;
using Borm.Model.Metadata;
using Moq;

namespace Borm.Tests.Mocks.Builders;

internal sealed class ColumnMetadataImplBuilder
{
    private readonly Mock<IColumnMetadata> _mock = new();
    private Constraints _constraints;

    public IColumnMetadata Build()
    {
        _mock.SetupGet(c => c.Constraints).Returns(_constraints);
        return _mock.Object;
    }

    public ColumnMetadataImplBuilder DataType(Type type, bool isNullable)
    {
        _mock.SetupGet(c => c.DataType).Returns(new Borm.Reflection.NullableType(type, isNullable));
        return this;
    }

    public ColumnMetadataImplBuilder Index(int index)
    {
        _mock.SetupGet(c => c.Index).Returns(index);
        return this;
    }

    public ColumnMetadataImplBuilder Name(string name)
    {
        _mock.SetupGet(c => c.Name).Returns(name);
        return this;
    }

    public ColumnMetadataImplBuilder Nullable()
    {
        if (_constraints.HasFlag(Constraints.PrimaryKey))
        {
            throw new InvalidOperationException("Primary keys cannot be nullable");
        }
        _constraints |= Constraints.AllowDbNull;

        return this;
    }

    public ColumnMetadataImplBuilder PrimaryKey()
    {
        if (_constraints.HasFlag(Constraints.AllowDbNull))
        {
            throw new InvalidOperationException("Primary keys cannot be nullable");
        }
        _constraints |= Constraints.PrimaryKey;

        return this;
    }

    public ColumnMetadataImplBuilder PropertyName(string propertyName)
    {
        _mock.SetupGet(c => c.PropertyName).Returns(propertyName);
        return this;
    }

    public ColumnMetadataImplBuilder Reference(Type reference)
    {
        _mock.SetupGet(c => c.Reference).Returns(reference);
        return this;
    }

    public ColumnMetadataImplBuilder Unique()
    {
        _constraints |= Constraints.Unique;
        return this;
    }
}
