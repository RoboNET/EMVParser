using System.Text;

namespace RoboNet.EMVParser;

public static class PrefferedApplicationNameUtility
{
    static PrefferedApplicationNameUtility()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    
    /// <summary>
    /// Get application preffered name with issuer code table
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static string? GetApplicationPrefferedName(this IReadOnlyList<TagPointer> tags)
    {
        var tag = tags.GetTag(EMVTags.ApplicationPreferredName);

        if (tag == null)
            return null;
        
        var code = tags.GetTag(EMVTags.IssuerCodeTableIndex);
        if (code == null)
        {
            return tag.ValueString;
        }
        
        Encoding iso8859_5 = Encoding.GetEncoding($"ISO-8859-{code.ValueNumeric}");
        return iso8859_5.GetString(tag.Value.Span);
    }
}