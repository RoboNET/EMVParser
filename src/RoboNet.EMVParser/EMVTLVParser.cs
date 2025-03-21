namespace RoboNet.EMVParser;

public static partial class EMVTLVParser
{
    /// <summary>
    /// Get value of specified tag from TLV data
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Bytes of Tag</param>
    /// <returns>Value of specified tag or empty data</returns>
    [MemoryVariantMethodGenerator]
    public static Memory<byte> GetTagValue(Memory<byte> data, byte[] tagKey)
    {
        var slice = data;

        while (!slice.IsEmpty)
        {
            var tagRange = ParseTagRange(slice, out var skipBytes, out var dataType, out _);
            var length = ParseTagLength(slice.Slice(skipBytes), out var lengthSkipByts);

            var tagRangeData = tagRange.GetOffsetAndLength(slice.Length);
            var tag = slice.Slice(tagRangeData.Offset, tagRangeData.Length);

            if (dataType == DataType.ConstructedDataObject)
            {
                var value = slice.Slice(skipBytes + lengthSkipByts, length);
                var internalTag = GetTagValue(value, tagKey);
                if (!internalTag.IsEmpty)
                    return internalTag;
            }

            if (tag.Span.SequenceEqual(tagKey))
            {
                return slice.Slice(skipBytes + lengthSkipByts, length);
            }

            slice = slice.Slice(skipBytes + lengthSkipByts + length);
        }

        return Array.Empty<byte>();
    }

    /// <summary>
    /// Get value of specified tag from TLV data
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    [MemoryVariantMethodGenerator]
    public static Memory<byte> GetTagValue(Memory<byte> data, string tagKey)
    {
        var comparer = Convert.FromHexString(tagKey);
        return GetTagValue(data, comparer);
    }

    /// <summary>
    /// Get value in HEX of specified tag from TLV data
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    [MemoryVariantMethodGenerator]
    public static string GetHexTagValue(Memory<byte> data, string tagKey)
    {
        var comparer = Convert.FromHexString(tagKey);
        var result = GetTagValue(data, comparer);
        if (result.IsEmpty)
            return string.Empty;

        return Convert.ToHexString(result.Span);
    }

    /// <summary>
    /// Get value in HEX of specified tag from TLV data
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static string GetHexTagValue(byte[] data, string tagKey)
    {
        return GetHexTagValue(data.AsSpan(), tagKey);
    }

    /// <summary>
    /// Get value in HEX of specified tag from TLV data
    /// </summary>
    /// <param name="data">TLV in HEX</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static string GetHexTagValue(string data, string tagKey)
    {
        return GetHexTagValue(Convert.FromHexString(data), tagKey);
    }

    /// <summary>
    /// Get value of specified tag from TLV data
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static Span<byte> GetTagValue(byte[] data, string tagKey)
    {
        return GetTagValue(data.AsSpan(), tagKey);
    }

    /// <summary>
    /// Get list of TLV from HEX TLV
    /// </summary>
    /// <param name="data">HEX string of TLV</param>
    /// <returns>List of TLV tags</returns>
    public static IReadOnlyList<TagPointerReadonly> ParseTagsList(string data)
    {
        return ParseTagsList(new ReadOnlyMemory<byte>(Convert.FromHexString(data)));
    }
    
    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <returns>List of TLV tags</returns>
    public static IReadOnlyList<TagPointer> ParseTagsList(byte[] data)
    {
        return ParseTagsList(data.AsMemory());
    }

    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <returns>List of TLV tags</returns>
    [MemoryVariantMethodGenerator(MemoryVariantGeneration.Memory)]
    public static IReadOnlyList<TagPointer> ParseTagsList(Memory<byte> data)
    {
        var slice = data;

        var pointers = new List<TagPointer>();

        while (!slice.IsEmpty)
        {
            var tagRange = ParseTagRange(slice, out var skipBytes, out var dataType, out var classType);
            var length = ParseTagLength(slice.Slice(skipBytes), out var lengthSkipByts);
            var value = slice.Slice(skipBytes + lengthSkipByts, length);

            IReadOnlyList<TagPointer> internalTags = dataType == DataType.PrimitiveDataObject
                ? new List<TagPointer>()
                : ParseTagsList(value);

            var tagData = tagRange.GetOffsetAndLength(slice.Length);
            pointers.Add(new TagPointer()
            {
                FullTagData = slice.Slice(0, skipBytes + lengthSkipByts + length),
                Tag = slice.Slice(tagData.Offset, tagData.Length),
                Value = value,
                Length = length,
                InternalTags = internalTags,
                TagDataType = dataType,
                TagClassType = classType
            });

            slice = slice.Slice(skipBytes + lengthSkipByts + length);
        }

        return pointers;
    }

    internal static int ParseTagLength(ReadOnlyMemory<byte> data, out int skipBytes)
    {
        return ParseTagLength(data.Span, out skipBytes);
    }

    internal static int ParseTagLength(ReadOnlySpan<byte> data, out int skipBytes)
    {
        var firstLengthByte = (int)data[0];

        var lengthFirstBit =
            ((byte)(firstLengthByte >> 7)) << 7; //Get first bit of length byte

        if (lengthFirstBit == 0)
        {
            skipBytes = 1;
            return data[0];
        }
        else
        {
            var lengthSize =
                ((byte)(firstLengthByte << 1)) >> 1; //Remove 1 bit from left

            skipBytes = 1 + lengthSize;
            var arr = new byte[4];

            for (int i = 0; i < lengthSize; i++)
            {
                arr[i] = data[1 + i];
            }

            data.Slice(1, lengthSize).ToArray();

            return BitConverter.ToInt32(arr);
        }
    }

    internal static Range ParseTagRange(ReadOnlyMemory<byte> data,
        out int skipBytes,
        out DataType dataType,
        out ClassType classType)
    {
        return ParseTagRange(data.Span, out skipBytes, out dataType, out classType);
    }

    internal static Range ParseTagRange(ReadOnlySpan<byte> data,
        out int skipBytes,
        out DataType dataType,
        out ClassType classType)
    {
        var firstByte = (int)data[0];
        var tagType = (byte)(firstByte << 3) >> 3; //Remove left 3 bits to get tag type
        classType = (ClassType)(firstByte >> 6); //Get last 2 bits to get class type
        dataType = (DataType)((firstByte & (1 << 5)) == 0
            ? 0
            : 1); //Check 5-th bit to get data type

        if (tagType == 31)
        {
            skipBytes = 2;
            return new Range(0, 2);
        }
        else
        {
            skipBytes = 1;
            return new Range(0, 1);
        }
    }
}