namespace Borm.Model.Metadata;

internal abstract class MetadataFactory<TSource, TMetadata>
    where TMetadata : IMetadata
{
    public abstract TMetadata Create(TSource source);

    protected static string CreateDefaultName(string memberName)
    {
        char first = memberName[0];
        if (!char.IsUpper(first))
        {
            return memberName;
        }

        return memberName.Length == 1
            ? char.ToLower(first).ToString()
            : char.ToLower(first) + memberName[1..];
    }
}