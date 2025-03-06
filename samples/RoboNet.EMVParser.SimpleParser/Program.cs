// See https://aka.ms/new-console-template for more information

using RoboNET.EMVParser;

Console.WriteLine("Enter emv tags in HEX");

var emvTags = Console.ReadLine();

var data = Convert.FromHexString(emvTags!);

var tagsList = EMVTLVParser.ParseTagsList(data);

foreach (var tag in tagsList)
{
    WriteTag(tag, 0);
}

void WriteTag(TagPointer tag, int offset)
{
    var offsetString = new string('-', offset * 2) + " ";
    Console.WriteLine(offsetString + "TAG:" + Convert.ToHexString(tag.TagData.Span));
    if (tag.InternalTags.Count > 0)
    {
        Console.WriteLine(offsetString + "Internal tags:");
        foreach (var internalTag in tag.InternalTags)
        {
            WriteTag(internalTag, offset + 1);
        }
    }
    else
    {
        Console.WriteLine(offsetString + "VALUE:" + Convert.ToHexString(tag.ValueData.Span));
    }
}