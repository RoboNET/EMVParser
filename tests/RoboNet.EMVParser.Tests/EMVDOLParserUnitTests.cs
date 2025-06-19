namespace RoboNet.EMVParser.Tests;

public class DOLParserUnitTests
{
    [Theory]
    [InlineData("9F02069F1D02", 2, 6)]
    public void TestParseTag(string sample,
        int expetedCount, int expectedFirstTagLength)
    {
        var tags = DOLParser.ParseTagsList(sample);

        Assert.Equal(expetedCount, tags.Count);
        Assert.Equal(expectedFirstTagLength, tags.First().Length);
    }
}