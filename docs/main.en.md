# RoboNet.EMVParser

[![NuGet Version](https://img.shields.io/nuget/v/RoboNet.EMVParser.svg?style=flat)](https://www.nuget.org/packages/RoboNet.EMVParser/)

RoboNet.EMVParser is a high-performance library for working with EMV data in TLV format, optimized for use in .NET applications.

## Key Features

- 🚀 High-performance parsing using Span API
- 📦 Support for both Memory<byte> and Span<byte>
- 🔍 Automatic detection and parsing of constructed tags
- 🎯 Support for long tags and multi-byte lengths
- 🛠️ Convenient extension methods for working with tag lists
- 💡 Performance optimization and minimal memory allocation

## Installation

```bash
dotnet add package RoboNet.EMVParser
```

## Quick Start

```csharp
// Example EMV data
var emvData = "9F2608AABBCCDDEE12345695050000000000";
var data = Convert.FromHexString(emvData);

// Get all tags
IReadOnlyList<TagPointer> tagsList = EMVTLVParser.ParseTagsList(data);

// Find specific tag
var tag = tagsList.GetTag("9F26");
if (tag != null)
{
    Console.WriteLine($"Tag value: {tag.ValueHex}");
}
```

# Working with TLV Tags

TLV (Tag-Length-Value) is a data format used in EMV transactions. The RoboNet.EMVParser library provides a convenient API for working with this format.

## Basic Concepts

TLV consists of three parts:
- Tag - data identifier
- Length - value length in bytes
- Value - the data itself

## BER-TLV Format

BER-TLV (Basic Encoding Rules - Tag Length Value) is a specific data encoding format used in EMV. It is based on ASN.1 Basic Encoding Rules and has the following features:

### Tag Structure
- The first byte contains information about:
  - Tag class (bits 7-8)
  - Data type (bit 6): primitive or constructed
  - Tag number (bits 1-5)
- If the tag number = 31 (11111), the tag continues in the next byte

### Length Structure
- If the first bit = 0: the following 7 bits contain the length
- If the first bit = 1: the following 7 bits indicate the number of bytes that contain the length

### Value
- For primitive tags: contains data
- For constructed tags: contains nested TLV structures

### Example
```
Tag: 9F 26 (two bytes)
Length: 08 (one byte)
Value: 01 23 45 67 89 AB CD EF (8 bytes)
```

## Long Tags and Lengths

### Multi-byte Tags
If the first 5 bits of the tag are '11111' (31 in decimal), it means the tag continues in the next byte. Format of subsequent bytes:
- Bit 8: if 1, tag continues in next byte
- Bits 7-1: part of tag number

Example:
```
1F 81 80 - three-byte tag, where:
1F = first byte (11111 in lower bits)
81 = second byte (1 in highest bit means continuation)
80 = last byte (0 in highest bit means end of tag)
```

### Multi-byte Lengths
If the first bit of the length byte is 1, the remaining 7 bits indicate the number of following bytes that contain the length value.

Examples:
```
82 01 00 - three-byte length, where:
82 = first byte (1 in highest bit, 2 in remaining bits means 2 length bytes)
01 00 = length value (256 in decimal)

81 FF - two-byte length, where:
81 = first byte (1 in highest bit, 1 in remaining bits means 1 length byte)
FF = length value (255 in decimal)
```

### Library Processing
RoboNet.EMVParser library automatically handles multi-byte tags and lengths:
```csharp
// Example with long tag
var longTagData = "1F818001020304"; // Tag: 1F8180, Length: 01, Value: 02 03 04
var parsedTag = EMVTLVParser.ParseTagsList(Convert.FromHexString(longTagData));

// Example with long length
var longLengthData = "9F26820100" + new string('FF', 256); // 256 bytes of data
var parsedLength = EMVTLVParser.ParseTagsList(Convert.FromHexString(longLengthData));
```

## Main Classes

### TagPointer

`TagPointer` is the main class for working with TLV data. Contains the following properties:

- `TLV` - complete TLV data
- `Tag` - tag bytes
- `Value` - value bytes
- `Length` - value length
- `InternalTags` - internal tags (for constructed tags)
- `TagDataType` - tag data type
- `TagClassType` - tag class type

Additional convenience properties:
- `TLVHex` - complete TLV data in HEX format
- `TagHex` - tag in HEX format
- `ValueHex` - value in HEX format
- `ValueString` - value in ASCII encoding
- `ValueNumeric` - value as a number

## Main Methods

### Parsing All Tags

```csharp
var data = Convert.FromHexString(emvData);
IReadOnlyList<TagPointer> tagsList = EMVTLVParser.ParseTagsList(data);

foreach (var tag in tagsList)
{
    Console.WriteLine($"Tag: {tag.TagHex}, Value: {tag.ValueHex}");
}
```

### Finding a Specific Tag

```csharp
// Search in tag list
var tag = tagsList.GetTag("5F2A");
if (tag != null)
{
    Console.WriteLine($"Found tag value: {tag.ValueHex}");
}

// Direct search in data
var tagValue = EMVTLVParser.GetTagValue(data, "5F2A");
Console.WriteLine($"Tag value: {Convert.ToHexString(tagValue.Span)}");
```

### Getting Values

```csharp
// Get value in HEX
string hexValue = tagsList.GetTagValueHex("5F2A");

// Get value as byte array
Memory<byte> byteValue = tagsList.GetTagValue("5F2A");

// Get string value (for ASCII data)
string stringValue = tag.ValueString;

// Get numeric value
long numericValue = tag.ValueNumeric;
```

## Tag Types

Tags can be of two types:
- `PrimitiveDataObject` - contains simple data
- `ConstructedDataObject` - contains other TLV structures (accessible via `InternalTags`)

## Tag Classes

Tags can belong to different classes:
- `ApplicationClass`
- `ContextSpecificClass`
- And others defined in `ClassType`

## Notes

1. The library is optimized for working with Span API
2. Supports working with both Memory<byte> and Span<byte>
3. Provides convenient extension methods for working with tag lists
4. Automatically detects constructed tags and parses their internal structure 

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Support

If you encounter any issues or have suggestions for improving the library, please create an issue in the project repository.

## Working with Application Preferred Name

In EMV, the `9F12` tag (Application Preferred Name) is often used in conjunction with the `9F11` tag (Issuer Code Table Index) for proper application name display. The library provides a convenient way to retrieve this value with correct encoding through the `PreferredApplicationNameUtility` class.

### Usage Example

```csharp
// Get tag list from EMV data
var data = Convert.FromHexString(emvData);
var tagsList = EMVTLVParser.ParseTagsList(data);

// Get application preferred name with proper encoding
string? applicationName = tagsList.GetApplicationPreferredName();
// Example: "МИР Классик" or "Visa Debit"
```

### How It Works

1. The method looks for the `9F12` tag (Application Preferred Name) in the tag list
2. If the `9F11` tag (Issuer Code Table Index) is found, its value is used to determine the correct ISO-8859-X encoding
3. If the encoding tag is not found, ASCII encoding is used
4. Returns the decoded application name value or null if the tag is not found

### Notes

- The method automatically handles different encodings, which is especially important for cards with non-Latin characters
- Supports all standard ISO-8859 encodings
- Particularly useful for correct application name display on payment terminals 

## Working with EMVTags Class

The `EMVTags` class provides a convenient way to work with EMV tags using predefined constants and tag descriptions. The class is automatically generated based on the well-known-tags.txt file, which contains standard EMV tags and their descriptions.

### Getting Tag Description

To get a tag description by its number, use the `GetTagName` method:

```csharp
var tagName = EMVTags.GetTagName("5F2A");
// Returns: "Transaction Currency Code"
```

### Using Predefined Constants

Instead of using string literals for tags, you can use predefined constants:

```csharp
// Using constant instead of string literal
var tag = tagsList.GetTag(EMVTags.TransactionCurrencyCode); // instead of "5F2A"
if (tag != null)
{
    Console.WriteLine($"Currency tag value: {tag.ValueHex}");
}
```

### Benefits of Using EMVTags

1. **Type Safety**: Using predefined constants instead of string literals helps avoid typos
2. **Documentation**: Each constant contains XML documentation describing the tag's purpose
3. **Maintainability**: When tag numbers change, only well-known-tags.txt needs to be updated
4. **IntelliSense**: IDE provides autocompletion and hints when working with tags

### Complex Usage Example

```csharp
var emvData = "9F2608AABBCCDDEE12345695050000000000";
var data = Convert.FromHexString(emvData);
var tagsList = EMVTLVParser.ParseTagsList(data);

// Getting tag value using constant
var applicationCryptogram = tagsList.GetTag(EMVTags.ApplicationCryptogram);

// Getting tag description
var tagDescription = EMVTags.GetTagName(applicationCryptogram.TagHex);

// Output information
Console.WriteLine($"Tag: {applicationCryptogram.TagHex}");
Console.WriteLine($"Description: {tagDescription}");
Console.WriteLine($"Value: {applicationCryptogram.ValueHex}");
``` 