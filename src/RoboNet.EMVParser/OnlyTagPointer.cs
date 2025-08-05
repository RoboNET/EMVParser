using System.Diagnostics;

namespace RoboNet.EMVParser;

/// <summary>
/// Only tag container with parsed data
/// </summary>
[DebuggerDisplay("{DebugText}")]
[OnlyTagPointerGeneration]
public class OnlyTagPointer
{
    /// <summary>
    /// Bytes of Tag part
    /// </summary>
    public required Memory<byte> Tag { get; init; }

    /// <summary>
    /// Tag part in HEX
    /// </summary>
    public string TagHex => Convert.ToHexString(Tag.Span);

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
    /// Full Tag data in HEX. Same as <see cref="TagHex"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return TagHex;
    }

    [DebuggerHidden]
    private string DebugText => ToStringInternal();

    [DebuggerHidden]
    private string ToStringInternal()
    {
        return
            $"Tag: {Convert.ToHexString(Tag.Span)} {Name}";
    }
}