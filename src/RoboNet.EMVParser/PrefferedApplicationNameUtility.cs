using System.Text;

namespace RoboNet.EMVParser;

public static class PreferredApplicationNameUtility
{
    static PreferredApplicationNameUtility()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Get application preferred name (9F12) with issuer code table
    /// </summary>
    /// <param name="tags">Tags list to get value</param>
    /// <returns>application preferred name</returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static string? GetApplicationPreferredName(this IReadOnlyList<TagPointer> tags)
    {
        var tag = tags.GetTag(EMVTags.ApplicationPreferredName);

        if (tag == null)
            return null;

        var code = tags.GetTag(EMVTags.IssuerCodeTableIndex);
        if (code == null)
        {
            return tag.ValueString;
        }

        Encoding iso8859 = Encoding.GetEncoding($"ISO-8859-{code.ValueNumeric}");
        return iso8859.GetString(tag.Value.Span);
    }
}