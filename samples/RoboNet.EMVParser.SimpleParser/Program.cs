// See https://aka.ms/new-console-template for more information

using RoboNet.EMVParser;

Console.WriteLine("Enter emv tags in HEX");

var emvTags = Console.ReadLine();

var data = Convert.FromHexString(emvTags!);

var tagsList = TLVParser.ParseTagsList(data);

foreach (var tag in tagsList)
{
    WriteTag(tag, 0);
}

void WriteTag(TagPointer tag, int offset)
{
    var offsetString = new string('-', offset * 2) + " ";
    var tagName = EMVTags.GetTagName(tag.TagHex);
    Console.WriteLine(offsetString + "TAG:" + tag.TagHex + (tagName != null ? " (NAME:" + tagName + ")" : ""));

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
        Console.WriteLine(offsetString + "VALUE:" + tag.ValueHex);
    }
}