using System.Diagnostics;

namespace RoboNet.EMVParser;

[DebuggerDisplay("{DebugText}")]
public class TagPointer
{
    /// <summary>
    /// Full TLV data
    /// </summary>
    public required Memory<byte> FullTagData { get; init; }
    
    /// <summary>
    /// Bytes of Tag part
    /// </summary>
    public required Memory<byte> TagData { get; init; }
    
    /// <summary>
    /// Bytes of Value part
    /// </summary>
    public required Memory<byte> ValueData { get; init; }
    
    /// <summary>
    /// Tag part in HEX
    /// </summary>
    public string Tag => Convert.ToHexString(TagData.Span);
    
    /// <summary>
    /// Value part in HEX
    /// </summary>
    public string Value => Convert.ToHexString(ValueData.Span);
    
    /// <summary>
    /// Length
    /// </summary>
    public required int Length { get; init; }
    
    /// <summary>
    /// If tag is constructed data type, contains internal tags
    /// </summary>
    public required IReadOnlyList<TagPointer> InternalTags { get; init; } = new List<TagPointer>();

    public override string ToString()
    {
        return Convert.ToHexString(FullTagData.Span);
    }

    [DebuggerHidden]
    private string DebugText => ToStringInternal();

    private string ToStringInternal()
    {
        if (InternalTags.Count > 0)
        {
            return $"Tag: {Convert.ToHexString(TagData.Span)} ({Convert.ToHexString(FullTagData.Span)}) " +
                   Environment.NewLine + "Internal tags:" +
                   string.Join(Environment.NewLine, InternalTags.Select(x => x.ToStringInternal()));
        }
        else
        {
            return
                $"Tag: {Convert.ToHexString(TagData.Span)}, Value: {Convert.ToHexString(ValueData.Span)} ({Convert.ToHexString(FullTagData.Span)})";
        }
    }
}