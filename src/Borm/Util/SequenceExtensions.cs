namespace Borm.Util;

internal static class SequenceExtensions
{
    public static int GetSequenceHashCode<T>(this IEnumerable<T> sequence)
    {
        const int seed = 487;
        const int modifier = 31;
        unchecked
        {
            return sequence
                .Where(item => item is not null)
                .Aggregate(seed, (current, item) => (current * modifier) + item!.GetHashCode());
        }
    }
}