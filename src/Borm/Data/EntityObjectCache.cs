namespace Borm.Data;

internal sealed class EntityObjectCache
{
    private readonly Dictionary<object, object> _pkEntityMap = [];

    public IEnumerable<object> Entities => _pkEntityMap.Values;

    public void Add(object primaryKey, object entity)
    {
        _ = _pkEntityMap.TryAdd(primaryKey, entity);
    }

    public void Remove(object primaryKey)
    {
        _ = _pkEntityMap.Remove(primaryKey);
    }

    public object? Find(object primaryKey)
    {
        if (_pkEntityMap.TryGetValue(primaryKey, out object? entity))
        {
            return entity;
        }
        return null;
    }

    public void Update(object primaryKey, object newEntity)
    {
        if (_pkEntityMap.Remove(primaryKey))
        {
            _pkEntityMap.Add(primaryKey, newEntity);
        }
    }
}
