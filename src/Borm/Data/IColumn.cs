using Borm.Model;

namespace Borm.Data;

public interface IColumn
{
    public string Name { get; }
    public Type DataType { get; }
    public Constraints Constraints { get; }
}
