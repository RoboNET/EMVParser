# EMV Parser [![NuGet Version](https://img.shields.io/nuget/v/RoboNet.EMVParser.svg?style=flat)](https://www.nuget.org/packages/RoboNet.EMVParser/)&nbsp;

Span API based EMV TLV and DOL parser for .NET. This library is designed to parse EMV data structures, specifically the BER-TLV (Tag-Length-Value) format used in EMV transactions and DOL (Data Object List) structures.

## Features

- 🚀 High-performance parsing using Span API
- 📦 Support for both Memory<byte> and Span<byte>
- 🔍 Automatic detection and parsing of constructed tags
- 🎯 Support for long tags and multi-byte lengths
- 🛠️ Convenient extension methods for working with tag lists
- 💡 Performance optimization and minimal memory allocation
- 🔢 DOL (Data Object List) parsing support for CDOL1, CDOL2, and DDOL structures
- 📝 Built-in EMV tags dictionary with descriptions and strongly-typed constants
- 📝 Comprehensive documentation in [English](docs/main.en.md) and [Russian](docs/main.ru.md)

## Installation

```bash
dotnet add package RoboNet.EMVParser
```

## Quick Start

Get all tags from EMV data:
```csharp
var someEMVData = "5F2A02097882021C00950580800088009A032110149C01009F02060000000020219F03060000000000009F0902008C9F100706010A03A480109F1A0202769F26080123456789ABCDEF9F2701809F3303E0F0C89F34034103029F3501229F3602003E9F37040F00BA209F41030010518407A0000000031010";
var data = Convert.FromHexString(someEMVData);

// Get all tags
IReadOnlyList<TagPointer> tagsList = TLVParser.ParseTagsList(data);

foreach (var tag in tagsList)
{
    Console.WriteLine($"Tag: {tag.TagHex}, Value: {tag.ValueHex}");
}
```

Get specific tag value from EMV data:
```csharp
// Get value of tag 5F2A
Span<byte> tagValue = TLVParser.ReadTagValue(data.AsSpan(), "5F2A"); 
Console.WriteLine("Tag 5F2A value: " + Convert.ToHexString(tagValue));
```

Parse DOL (Data Object List) structures:
```csharp
// Parse CDOL1 or DDOL structure - contains tag-length pairs without values
var dolData = "9F02069F1D029F03069F1A0295055F2A029A039C019F37049F21039F7C14";
IReadOnlyList<TagLengthPointer> dolTags = DOLParser.ParseTagsList(dolData);

foreach (var dolTag in dolTags)
{
    Console.WriteLine($"Tag: {dolTag.TagHex}, Expected Length: {dolTag.Length}");
}
```

## Main Features

### TagPointer Class
The main class for working with TLV data provides:
- Complete TLV data access
- Tag and value manipulation
- Support for constructed tags
- Convenient hex and string value representations
- Numeric value conversion

### Working with Tags
```csharp
// Search in tag list
var tag = tagsList.GetTag("5F2A");

// Get value in different formats
string hexValue = tagsList.GetTagValueHex("5F2A");
Memory<byte> byteValue = tagsList.GetTagValue("5F2A");
string stringValue = tag.ValueString;
long numericValue = tag.ValueNumeric;
```

## Documentation

For detailed information about working with the library, please refer to the documentation:
- [English Documentation](docs/main.en.md)
- [Russian Documentation](docs/main.ru.md)

## License

This project is licensed under the MIT License - see the LICENSE file for details.