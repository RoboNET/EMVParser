using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoboNET.EMVParser.SourceGenerators;

[Generator]
public class SampleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        // define the execution pipeline here via a series of transformations:

        // find all additional files that end with .txt
        IncrementalValuesProvider<AdditionalText> textFiles =
            initContext.AdditionalTextsProvider.Where(file => file.Path.EndsWith("well-known-tags.txt"));

        // read their contents and save their name
        IncrementalValuesProvider<(string name, string content)> namesAndContents =
            textFiles.Select((text, cancellationToken) => (name: Path.GetFileNameWithoutExtension(text.Path),
                content: text.GetText(cancellationToken)!.ToString()));

        // generate a class that contains their values as const strings
        initContext.RegisterSourceOutput(namesAndContents, (spc, nameAndContent) =>
        {
            Dictionary<string, int> processedTags = new Dictionary<string, int>();
            StringBuilder builder = new StringBuilder();

            StringBuilder tagNamesBuilder = new StringBuilder();

            var lines = nameAndContent.content.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(';');

                if (parts.Length != 3)
                    continue;

                tagNamesBuilder.AppendLine($"{{\"{parts[0].Trim('"').Trim()}\",\"{parts[1].Trim('"').Trim()}\"}},");

                builder.AppendLine($"        /// <summary>");
                builder.AppendLine($"        /// {parts[2].Trim('"').Trim()}");
                builder.AppendLine($"        /// </summary>");

                var tagName = parts[1].Trim('"').Trim().Replace(" ", "").Replace(",", "").Replace("-", "")
                    .Replace("–", "").Replace("(", "").Replace(")", "");
                
                builder.AppendLine(
                    $"        public readonly static string {tagName} = \"{parts[0].Trim('"').Trim()}\";");
            }

            spc.AddSource($"EMVTags.g", SourceText.From($@"
    // <auto-generated/>

#nullable enable
    using System.Collections.Generic;
    public partial class EMVTags
    {{
        private static Dictionary<string, string> tags = new Dictionary<string, string>()
        {{
            {tagNamesBuilder.ToString()}
        }};

        public static string? GetTagName(string tag)
        {{
            if (tags.TryGetValue(tag, out var description))
            {{
                return description;
            }}
            return null;
        }}

{builder.ToString()}
    }}
#nullable disable", Encoding.UTF8));
        });
    }
}