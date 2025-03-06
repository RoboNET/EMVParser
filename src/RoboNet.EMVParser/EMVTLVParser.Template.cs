namespace RoboNet.EMVParser;

public static partial class EMVTLVParser
{

    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Bytes of Tag</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static ReadOnlySpan<byte> ReadTagValue(ReadOnlySpan<byte> data, byte[] tagKey)
    {
        var slice = data;
    
        while (!slice.IsEmpty)
        {
            var tagRange = ParseTagRange(slice, out var skipBytes, out var dataType, out var classType);
            var length = ParseTagLength(slice.Slice(skipBytes), out var lengthSkipByts);
    
            var tagRangeData = tagRange.GetOffsetAndLength(slice.Length);
            var tag = slice.Slice(tagRangeData.Offset, tagRangeData.Length); 
            
            if (dataType == DataType.ConstructedDataObject)
            {
                var value = slice.Slice(skipBytes + lengthSkipByts, length);
                var internalTag = ReadTagValue(value, tagKey);
                if (!internalTag.IsEmpty)
                    return internalTag;
            }
    
            if (tag.SequenceEqual(tagKey))
            {
                return slice.Slice(skipBytes + lengthSkipByts, length);
            }
    
            slice = slice.Slice(skipBytes + lengthSkipByts + length);
        }
    
        return Array.Empty<byte>();
    }

    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static ReadOnlySpan<byte> ReadTagValue(ReadOnlySpan<byte> data, string tagKey)
    {
        var comparer = Convert.FromHexString(tagKey);
        return ReadTagValue(data, comparer);
    }

    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Bytes of Tag</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static Span<byte> ReadTagValue(Span<byte> data, byte[] tagKey)
    {
        var slice = data;
    
        while (!slice.IsEmpty)
        {
            var tagRange = ParseTagRange(slice, out var skipBytes, out var dataType, out var classType);
            var length = ParseTagLength(slice.Slice(skipBytes), out var lengthSkipByts);
    
            var tagRangeData = tagRange.GetOffsetAndLength(slice.Length);
            var tag = slice.Slice(tagRangeData.Offset, tagRangeData.Length); 
            
            if (dataType == DataType.ConstructedDataObject)
            {
                var value = slice.Slice(skipBytes + lengthSkipByts, length);
                var internalTag = ReadTagValue(value, tagKey);
                if (!internalTag.IsEmpty)
                    return internalTag;
            }
    
            if (tag.SequenceEqual(tagKey))
            {
                return slice.Slice(skipBytes + lengthSkipByts, length);
            }
    
            slice = slice.Slice(skipBytes + lengthSkipByts + length);
        }
    
        return Array.Empty<byte>();
    }

    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static Span<byte> ReadTagValue(Span<byte> data, string tagKey)
    {
        var comparer = Convert.FromHexString(tagKey);
        return ReadTagValue(data, comparer);
    }

    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Bytes of Tag</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static ReadOnlyMemory<byte> ReadTagValue(ReadOnlyMemory<byte> data, byte[] tagKey)
    {
        var slice = data;
    
        while (!slice.IsEmpty)
        {
            var tagRange = ParseTagRange(slice, out var skipBytes, out var dataType, out var classType);
            var length = ParseTagLength(slice.Slice(skipBytes), out var lengthSkipByts);
    
            var tagRangeData = tagRange.GetOffsetAndLength(slice.Length);
            var tag = slice.Slice(tagRangeData.Offset, tagRangeData.Length); 
            
            if (dataType == DataType.ConstructedDataObject)
            {
                var value = slice.Slice(skipBytes + lengthSkipByts, length);
                var internalTag = ReadTagValue(value, tagKey);
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
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static ReadOnlyMemory<byte> ReadTagValue(ReadOnlyMemory<byte> data, string tagKey)
    {
        var comparer = Convert.FromHexString(tagKey);
        return ReadTagValue(data, comparer);
    }

    /// <summary>
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Bytes of Tag</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static Memory<byte> ReadTagValue(Memory<byte> data, byte[] tagKey)
    {
        var slice = data;
    
        while (!slice.IsEmpty)
        {
            var tagRange = ParseTagRange(slice, out var skipBytes, out var dataType, out var classType);
            var length = ParseTagLength(slice.Slice(skipBytes), out var lengthSkipByts);
    
            var tagRangeData = tagRange.GetOffsetAndLength(slice.Length);
            var tag = slice.Slice(tagRangeData.Offset, tagRangeData.Length); 
            
            if (dataType == DataType.ConstructedDataObject)
            {
                var value = slice.Slice(skipBytes + lengthSkipByts, length);
                var internalTag = ReadTagValue(value, tagKey);
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
    /// Get list of TLV from binary list
    /// </summary>
    /// <param name="data">Bytes of TLV</param>
    /// <param name="tagKey">Tag name</param>
    /// <returns>Value of specified tag or empty data</returns>
    public static Memory<byte> ReadTagValue(Memory<byte> data, string tagKey)
    {
        var comparer = Convert.FromHexString(tagKey);
        return ReadTagValue(data, comparer);
    }

}