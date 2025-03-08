namespace RoboNet.EMVParser;

public static class IReadOnlyListExtensions
{
    /// <summary>
    /// Search for tag in list of tags
    /// </summary>
    /// <param name="tags">Parsed tags</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Pointer to tag ot null, if tag not found</returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static TagPointer? GetTag(this IReadOnlyList<TagPointer> tags, string tagKey)
    {
        var comparer = Convert.FromHexString(tagKey);

        return GetTag(tags, comparer);
    }

    /// <summary>
    /// Search for tag in list of tags
    /// </summary>
    /// <param name="tags">Parsed tags</param>
    /// <param name="comparer">Tag name</param>
    /// <returns>Pointer to tag ot null, if tag not found</returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static TagPointer? GetTag(this IReadOnlyList<TagPointer> tags, byte[] comparer)
    {
        foreach (var tag in tags)
        {
            if (tag.TagData.Span.SequenceEqual(comparer))
            {
                return tag;
            }

            if (tag.InternalTags.Count > 0)
            {
                var internalTag = GetTag(tag.InternalTags, comparer);
                if (internalTag != null)
                    return internalTag;
            }
        }

        return null;
    }

    /// <summary>
    /// Get TLV value
    /// </summary>
    /// <param name="tags">Parsed tags</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Bytes of value</returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static Memory<byte> GetTagValue(this IReadOnlyList<TagPointer> tags, string tagKey)
    {
        var tag = GetTag(tags, tagKey);
        return tag?.ValueData ?? new Memory<byte>();
    }

    /// <summary>
    /// Get TLV value as hex string
    /// </summary>
    /// <param name="tags">Parsed tags</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>HEX value or empty string if tag not found</returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static string GetTagHexValue(this IReadOnlyList<TagPointer> tags, string tagKey)
    {
        var tag = GetTag(tags, tagKey);
        return tag?.Value ?? "";
    }
}