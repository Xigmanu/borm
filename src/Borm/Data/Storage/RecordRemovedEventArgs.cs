namespace Borm.Data.Storage;

internal sealed class RecordRemovedEventArgs : EventArgs
{
    public RecordRemovedEventArgs(object primaryKey)
    {
        PrimaryKey = primaryKey;
    }

    public object PrimaryKey { get; }
}
