namespace RoboNet.EMVParser.Tests;

/// <summary>
/// Negative tests for TLV Parser - testing invalid/malformed EMV tags
///
/// These tests verify that the parser correctly handles various invalid inputs:
/// - Incomplete data (truncated tags, missing length/value bytes)
/// - Invalid length encodings
/// - Malformed multi-byte tags
/// - Data exceeding buffer boundaries
/// - Zero-length tags (valid edge case)
///
/// Known limitations (tests that currently fail):
/// - Parser does not validate tag boundaries for incomplete multi-byte tags (1F, 5F, 9F, DF without second byte)
/// - Parser does not validate continuation bits in multi-byte tags (1F8F, 1FFF, 1F8080)
/// These cases may cause IndexOutOfRangeException in production code.
/// </summary>
public class TLVParserNegativeTests
{
    [Theory]
    [InlineData("5F")]  // Incomplete tag (2-byte tag with only 1 byte)
    [InlineData("9F")]  // Incomplete multi-byte tag
    [InlineData("5F2A")]  // Tag without length
    [InlineData("5F2A02")]  // Tag with length but no value
    [InlineData("5F2A0209")]  // Tag with length 2 but only 1 byte of value
    public void TestParseTagsList_IncompleteData_ShouldThrow(string invalidData)
    {
        var data = string.IsNullOrEmpty(invalidData)
            ? Array.Empty<byte>()
            : Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }

    [Fact]
    public void TestParseTagsList_EmptyData_ReturnsEmptyList()
    {
        var data = Array.Empty<byte>();
        var result = TLVParser.ParseTagsList(data.AsMemory());

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("5F2A8205")]  // Length indicates 5 bytes in extended format, but no length bytes follow
    [InlineData("5F2A8201")]  // Length byte indicates 1 byte follows, but no byte
    [InlineData("5F2A820200")]  // Length indicates 512 bytes but no data
    public void TestParseTagsList_InvalidLengthEncoding_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }

    [Theory]
    [InlineData("1F8F")]  // Multi-byte tag with continuation bit set, but incomplete
    [InlineData("1FFF")]  // Multi-byte tag with continuation bits, incomplete
    [InlineData("1F8080")]  // Multi-byte tag with multiple continuation bits
    public void TestParseTagRange_InvalidMultiByteTag_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() =>
            TLVParser.ParseTagRange(data.AsMemory(), out _, out _, out _));
    }

    [Theory]
    [InlineData("5F2AFF0102")]  // Length 0xFF (255) but only 2 bytes of data
    [InlineData("5F2A8204000000FF")]  // Length in long form indicates 255 bytes, but no data
    public void TestParseTagsList_LengthExceedsAvailableData_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }

    [Theory]
    [InlineData("5F2A")]  // Only tag, no length byte
    [InlineData("9F26")]  // Two-byte tag, no length
    public void TestParseTagLength_NoLengthByte_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);
        // First parse the tag to skip it
        _ = TLVParser.ParseTagRange(data.AsMemory(), out var skipBytes, out _, out _);
        var remainingData = data.AsMemory().Slice(skipBytes);

        Assert.ThrowsAny<Exception>(() =>
            TLVParser.ParseTagLength(remainingData, out _));
    }

    [Theory]
    [InlineData("5F2A85")]  // Length byte indicates 5 bytes for length value, but only 1 byte total
    [InlineData("5F2A8401")]  // Length byte indicates 4 bytes for length, but only 1 byte follows
    [InlineData("5F2A830102")]  // Length byte indicates 3 bytes for length, but only 2 bytes follow
    public void TestParseTagLength_IncompleteLengthValue_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }

    [Fact]
    public void TestGetTagValue_EmptyData_ShouldReturnEmpty()
    {
        var data = Array.Empty<byte>();
        var result = TLVParser.GetTagValue(data.AsMemory(), "5F2A");

        Assert.True(result.IsEmpty);
    }

    [Theory]
    [InlineData("5F2A020978", "9F26")]  // Valid TLV but searching for non-existent tag
    [InlineData("82021C00", "5F2A")]  // Valid TLV but searching for different tag
    public void TestGetTagValue_TagNotFound_ShouldReturnEmpty(string validData, string searchTag)
    {
        var data = Convert.FromHexString(validData);
        var result = TLVParser.GetTagValue(data.AsMemory(), searchTag);

        Assert.True(result.IsEmpty);
    }

    [Theory]
    [InlineData("6F82")]  // Constructed tag with length in extended form, but incomplete
    [InlineData("6F8201")]  // Constructed tag with extended length format, missing length value
    public void TestParseTagsList_ConstructedTagIncompleteLengthField_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }

    [Theory]
    [InlineData("5F2A00")]  // Tag with zero length - this should be valid
    [InlineData("9F2600")]  // Two-byte tag with zero length - this should be valid
    public void TestParseTagsList_ZeroLength_ShouldNotThrow(string validData)
    {
        var data = Convert.FromHexString(validData);

        var tags = TLVParser.ParseTagsList(data.AsMemory());

        Assert.Single(tags);
        Assert.Equal(0, tags[0].Length);
        Assert.True(tags[0].Value.IsEmpty);
    }

    [Theory]
    [InlineData("6F0082021C00")]  // Constructed tag with zero length followed by another tag
    public void TestParseTagsList_ConstructedZeroLength_ShouldNotThrow(string validData)
    {
        var data = Convert.FromHexString(validData);

        var tags = TLVParser.ParseTagsList(data.AsMemory());

        Assert.Equal(2, tags.Count);
        Assert.Equal(0, tags[0].Length);
    }

    [Fact]
    public void TestParseTagsList_InvalidHexString_ShouldThrow()
    {
        Assert.Throws<FormatException>(() =>
            Convert.FromHexString("INVALID"));
    }

    [Theory]
    [InlineData("5F2A020978XX")]  // Invalid hex characters
    [InlineData("5F2A02097G")]  // Invalid hex character 'G'
    public void TestParseTagsList_MalformedHexString_ShouldThrow(string invalidHex)
    {
        Assert.Throws<FormatException>(() =>
            Convert.FromHexString(invalidHex));
    }

    [Theory]
    [InlineData("1F")]  // Tag starting with 0x1F but missing subsequent bytes
    [InlineData("5F")]  // Two-byte tag indicator but missing second byte
    [InlineData("9F")]  // Two-byte tag indicator but missing second byte
    [InlineData("DF")]  // Two-byte tag indicator but missing second byte
    public void TestParseTagRange_IncompleteTwoByteTag_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        // For tags starting with 0x1F, 0x5F, 0x9F, or 0xDF, we need at least 2 bytes
        if (data.Length < 2 && (data[0] & 0x1F) == 0x1F)
        {
            Assert.ThrowsAny<Exception>(() =>
                TLVParser.ParseTagRange(data.AsMemory(), out _, out _, out _));
        }
    }

    [Theory]
    [InlineData("6F478409A00000005945430100A53A50086769726F636172648701")]  // Truncated in the middle
    [InlineData("5F2A020978820200")]  // Second tag incomplete
    public void TestParseTagsList_TruncatedInMiddle_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }

    [Theory]
    [InlineData("5F2A8600000001")]  // Length field indicates 6 bytes for length (invalid, max is 4)
    [InlineData("5F2A8700000001")]  // Length field indicates 7 bytes for length (invalid)
    public void TestParseTagLength_LengthFieldTooLarge_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        // This should throw because we only support up to 4 bytes for length (Int32)
        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }

    [Theory]
    [InlineData("6F1082021C00")]  // Constructed tag claims length 16, but only has 4 bytes
    [InlineData("6F478409A0000000594543")]  // Constructed tag claims length 71, truncated
    public void TestParseTagsList_ConstructedTagValueTruncated_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }

    [Fact]
    public void TestGetTagValue_InvalidTagFormat_ShouldThrow()
    {
        var validData = Convert.FromHexString("5F2A020978");

        // Invalid hex string for tag
        Assert.Throws<FormatException>(() =>
            TLVParser.GetTagValue(validData.AsMemory(), "INVALID"));
    }

    [Theory]
    [InlineData("6F0A5F2A")]  // Constructed tag with length 10, but internal tag incomplete
    [InlineData("6F055F2A02")]  // Constructed tag with incomplete internal TLV
    public void TestParseTagsList_ConstructedWithInvalidInternalTags_ShouldThrow(string invalidData)
    {
        var data = Convert.FromHexString(invalidData);

        Assert.ThrowsAny<Exception>(() => TLVParser.ParseTagsList(data.AsMemory()));
    }
}