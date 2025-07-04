using System.Diagnostics;
using System.Text;

namespace RoboNet.EMVParser;

/// <summary>
/// TLV container with parsed data
/// </summary>
[DebuggerDisplay("{DebugText}")]
[TagPointerGeneration]
public class TagPointer
{
    /// <summary>
    /// Full TLV data
    /// </summary>
    public required Memory<byte> TLV { get; init; }

    /// <summary>
    /// Full TLV data in HEX. Same as <see cref="ToString"/>
    /// </summary>
    public string TLVHex => Convert.ToHexString(TLV.Span);

    /// <summary>
    /// Bytes of Tag part
    /// </summary>
    public required Memory<byte> Tag { get; init; }

    /// <summary>
    /// Bytes of Value part
    /// </summary>
    public required Memory<byte> Value { get; init; }

    /// <summary>
    /// Tag part in HEX
    /// </summary>
    public string TagHex => Convert.ToHexString(Tag.Span);

    /// <summary>
    /// Value part in HEX
    /// </summary>
    public string ValueHex => Convert.ToHexString(Value.Span);

    /// <summary>
    /// Value part in ASCII encoding
    /// </summary>
    public string ValueString => Encoding.ASCII.GetString(Value.Span);

    /// <summary>
    /// Value part as numeric
    /// </summary>
    public long ValueNumeric => ParserUtils.ParseNumeric(Value.Span);

    /// <summary>
    /// Length
    /// </summary>
    public required int Length { get; init; }

    /// <summary>
    /// If tag is constructed data type, contains internal tags
    /// </summary>
    public required IReadOnlyList<TagPointer> InternalTags { get; init; } = new List<TagPointer>();

    /// <summary>
    /// Tag data type
    /// </summary>
    public required DataType TagDataType { get; init; }

    /// <summary>
    /// Tag class type
    /// </summary>
    public required ClassType TagClassType { get; init; }

    /// <summary>
    /// Get tag name
    /// </summary>
    public string? Name => EMVTags.GetTagName(TagHex);
    
    /// <summary>
    /// Full TLV data in HEX. Same as <see cref="TLVHex"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return TLVHex;
    }

    [DebuggerHidden]
    private string DebugText => ToStringInternal();

    [DebuggerHidden]
    private string ToStringInternal()
    {
        if (InternalTags.Count > 0)
        {
            return $"Tag: {Convert.ToHexString(Tag.Span)} {Name} ({Convert.ToHexString(TLV.Span)}) " +
                   Environment.NewLine + "Internal tags:" +
                   string.Join(Environment.NewLine, InternalTags.Select(x => x.ToStringInternal()));
        }
        else
        {
            return
                $"Tag: {Convert.ToHexString(Tag.Span)} {Name}, Value: {Convert.ToHexString(Value.Span)} ({Convert.ToHexString(TLV.Span)})";
        }
    }
}