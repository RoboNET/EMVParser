using System.Diagnostics;

namespace RoboNet.EMVParser;

/// <summary>
/// TL (DOL) container with parsed data
/// </summary>
[DebuggerDisplay("{DebugText}")]
[TagLengthPointerGeneration]
public class TagLengthPointer
{
    /// <summary>
    /// Full TL (DOL) data
    /// </summary>
    public required Memory<byte> TL { get; init; }

    /// <summary>
    /// Full TL (DOL) data in HEX. Same as <see cref="ToString"/>
    /// </summary>
    public string TLHex => Convert.ToHexString(TL.Span);

    /// <summary>
    /// Bytes of Tag part
    /// </summary>
    public required Memory<byte> Tag { get; init; }

    /// <summary>
    /// Tag part in HEX
    /// </summary>
    public string TagHex => Convert.ToHexString(Tag.Span);

    /// <summary>
    /// Length
    /// </summary>
    public required int Length { get; init; }

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
    /// Full TL data in HEX. Same as <see cref="TLHex"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return TLHex;
    }

    [DebuggerHidden]
    private string DebugText => ToStringInternal();

    [DebuggerHidden]
    private string ToStringInternal()
    {
        return
            $"Tag: {Convert.ToHexString(Tag.Span)} {Name} ({Convert.ToHexString(TL.Span)})";
    }
}