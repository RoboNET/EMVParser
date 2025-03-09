namespace RoboNet.EMVParser;

public static class ParserUtils
{
    public static long ParseNumeric(ReadOnlySpan<byte> data)
    {
        var result = 0;
        
        for (var i = data.Length-1; i >= 0; i--)
        {
            var pos = data.Length - 1 - i;
            result += (int)Math.Pow(10, (pos * 2) ) * (data[i] & 0x0F);
            result += (int)Math.Pow(10, (pos * 2) + 1) * (data[i] >> 4);
        }

        return result;
    }
}