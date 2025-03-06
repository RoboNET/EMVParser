namespace RoboNET.EMVParser;

public static partial class EMVTLVParser
{
    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <returns>List of TLV tags</returns>
    public static IReadOnlyList<TagPointerReadonly> ParseTagsList(ReadOnlyMemory<byte> data)
    {
        var slice = data;
        
#nullable enable
        var pointers = new List<TagPointerReadonly>();
#nullable disable
        while (!slice.IsEmpty)
        {
            var tagRange = ParseTagRange(slice, out var skipBytes, out var dataType, out var classType);
            var length = ParseTagLength(slice.Slice(skipBytes), out var lengthSkipByts);
            var value = slice.Slice(skipBytes + lengthSkipByts, length);

            IReadOnlyList<TagPointerReadonly> internalTags = dataType == DataType.PrimitiveDataObject
                ? new List<TagPointerReadonly>()
                : ParseTagsList(value);

            var tagData = tagRange.GetOffsetAndLength(slice.Length);

            pointers.Add(new TagPointerReadonly()
            {
                FullTagData = slice.Slice(0, skipBytes + lengthSkipByts + length),
                TagData = slice.Slice(tagData.Offset, tagData.Length),
                ValueData = value,
                Length = length,
                InternalTags = internalTags
            });

            slice = slice.Slice(skipBytes + lengthSkipByts + length);
        }

        return pointers;
    }

    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <returns>List of TLV tags</returns>
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
                TagData = slice.Slice(tagData.Offset, tagData.Length),
                ValueData = value,
                Length = length,
                InternalTags = internalTags
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
            var lendthSize =
                ((byte)(firstLengthByte << 1)) >> 1; //Remove 1 bit from left

            skipBytes = 1 + lendthSize;
            var arr = new byte[4];

            for (int i = 0; i < lendthSize; i++)
            {
                arr[i] = data[1 + i];
            }

            data.Slice(1, lendthSize).ToArray();

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