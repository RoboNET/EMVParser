namespace RoboNet.EMVParser;

/// <summary>
/// Parser for TL data format
/// </summary>
public static partial class DOLParser
{
    /// <summary>
    /// Get list of TLV from HEX TLV
    /// </summary>
    /// <param name="data">HEX string of TLV</param>
    /// <returns>List of TLV tags</returns>
    public static IReadOnlyList<TagLengthPointerReadonly> ParseTagsList(string data)
    {
        return ParseTagsList(new ReadOnlyMemory<byte>(Convert.FromHexString(data)));
    }

    /// <summary>
    /// Get list of TL from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <returns>List of TLV tags</returns>
    public static IReadOnlyList<TagLengthPointer> ParseTagsList(byte[] data)
    {
        return ParseTagsList(data.AsMemory());
    }

    /// <summary>
    /// Get list of TL from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <returns>List of TLV tags</returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static IReadOnlyList<TagLengthPointer> ParseTagsList(Memory<byte> data)
    {
        var slice = data;
        var pointers = new List<TagLengthPointer>();

        while (!slice.IsEmpty)
        {
            var tagRange = TLVParser.ParseTagRange(slice, out var skipBytes, out var dataType, out var classType);
            var length = TLVParser.ParseTagLength(slice.Slice(skipBytes), out var lengthSkipByts);

            var tagData = tagRange.GetOffsetAndLength(slice.Length);
            var tag = slice.Slice(tagData.Offset, tagData.Length);

            pointers.Add(new TagLengthPointer()
            {
                TL = slice.Slice(0, skipBytes + lengthSkipByts),
                Tag = tag,
                Length = length,
                TagDataType = dataType,
                TagClassType = classType
            });

            slice = slice.Slice(skipBytes + lengthSkipByts);
        }

        return pointers;
    }
}