namespace RoboNet.EMVParser.Tests;

public class TagListParserUnitTests
{
    [Theory]
    [InlineData("9F029F1A955F2A9A9C9F379F339F279F369F269F1082579F345F349F04", 17, "9F02")]
    public void TestParseTag(string sample,
        int expetedCount, string expectedFirstTagValue)
    {
        var tags = TagListParser.ParseTagsList(sample);

        Assert.Equal(expetedCount, tags.Count);
        Assert.Equal(expectedFirstTagValue, tags.First().TagHex);
    }
}