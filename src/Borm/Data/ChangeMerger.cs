using Borm.Properties;

namespace Borm.Data;

internal static class ChangeMerger
{
    public static List<Change> Merge(List<Change> original, List<Change> incoming)
    {
        Dictionary<object, Change> resultMap = original
            .GroupBy(c => c.Buffer.PrimaryKey)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (Change change in incoming)
        {
            object key = change.Buffer.PrimaryKey;
            if (resultMap.TryGetValue(key, out Change? existing))
            {
                Change? merged = existing.Merge(change, isCommit: true);
                if (merged != null)
                {
                    resultMap[key] = merged;
                }
                else
                {
                    resultMap.Remove(key);
                }
            }
            else
            {
                if (change.RowAction != RowAction.Insert)
                {
                    throw new InvalidOperationException(Strings.ModificationOfNonExistingRow());
                }
                resultMap[key] = change;
            }
        }

        return [.. resultMap.Values];
    }
}
