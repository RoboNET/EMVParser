# EMV Parser [![NuGet Version](https://img.shields.io/nuget/v/RoboNet.EMVParser.svg?style=flat)](https://www.nuget.org/packages/RoboNet.EMVParser/)&nbsp;

Span API based EMV TLV parser for .NET. This library is designed to parse EMV data structures, specifically the BER-TLV (Tag-Length-Value) format used in EMV transactions.

## Installation

```bash
dotnet add package RoboNet.EMVParser
```

## Usage

Get all tags from EMV data:
```csharp
var someEMVData = "5F2A02097882021C00950580800088009A032110149C01009F02060000000020219F03060000000000009F0902008C9F100706010A03A480109F1A0202769F26080123456789ABCDEF9F2701809F3303E0F0C89F34034103029F3501229F3602003E9F37040F00BA209F41030010518407A0000000031010";
var data = Convert.FromHexString(someEMVData);

//Get all tags
IReadOnlyList<TagPointer> tagsList = EMVTLVParser.ParseTagsList(data);

foreach (var tag in tagsList)
{
    Console.WriteLine($"Tag: {tag.Tag}, Value: {tag.Value}");
}
```

Get one tag value from EMV data:
```csharp
var someEMVData = "5F2A02097882021C00950580800088009A032110149C01009F02060000000020219F03060000000000009F0902008C9F100706010A03A480109F1A0202769F26080123456789ABCDEF9F2701809F3303E0F0C89F34034103029F3501229F3602003E9F37040F00BA209F41030010518407A0000000031010";
var data = Convert.FromHexString(someEMVData);

//Get value of tag 5F2A
Span<byte> tagsValue = EMVTLVParser.ReadTagValue(data.AsSpan(), "5F2A"); 

Console.WriteLine("Tag 5F2A value: " + Convert.ToHexString(tagsValue));
```