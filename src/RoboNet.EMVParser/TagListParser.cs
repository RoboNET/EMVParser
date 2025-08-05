namespace RoboNet.EMVParser;

/// <summary>
/// Parser for Tag list data format
/// </summary>
public static partial class TagListParser
{
    /// <summary>
    /// Get list of TLV from HEX TLV
    /// </summary>
    /// <param name="data">HEX string of TLV</param>
    /// <returns>List of TLV tags</returns>
    public static IReadOnlyList<OnlyTagPointerReadonly> ParseTagsList(string data)
    {
        return ParseTagsList(new ReadOnlyMemory<byte>(Convert.FromHexString(data)));
    }

    /// <summary>
    /// Get list of TL from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <returns>List of TLV tags</returns>
    public static IReadOnlyList<OnlyTagPointer> ParseTagsList(byte[] data)
    {
        return ParseTagsList(data.AsMemory());
    }

    /// <summary>
    /// Get list of TL from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <returns>List of TLV tags</returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static IReadOnlyList<OnlyTagPointer> ParseTagsList(Memory<byte> data)
    {
        var slice = data;
        var pointers = new List<OnlyTagPointer>();

        while (!slice.IsEmpty)
        {
            var tagRange = TLVParser.ParseTagRange(slice, out var skipBytes, out var dataType, out var classType);

            var tagData = tagRange.GetOffsetAndLength(slice.Length);
            var tag = slice.Slice(tagData.Offset, tagData.Length);

            pointers.Add(new OnlyTagPointer()
            {
                Tag = tag,
                TagDataType = dataType,
                TagClassType = classType
            });

            slice = slice.Slice(skipBytes);
        }

        return pointers;
    }
}