namespace Borm.Data.Storage.Tracking;

internal interface IMerger
{
    IChange? Merge(IChange existing, IChange incoming, MergeMode mode);
}