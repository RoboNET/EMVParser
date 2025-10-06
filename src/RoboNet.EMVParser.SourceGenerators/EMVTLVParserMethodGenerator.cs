using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoboNet.EMVParser.SourceGenerators;

[Generator]
public class MemoryVariantMethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var enumsToGenerate = initContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                "RoboNet.EMVParser.MemoryVariantMethodGeneratorAttribute",
                predicate: (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) => ctx.TargetNode);


        initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "MemoryVariantMethodGeneratorAttribute.g.cs",
            SourceText.From(@"
using System.Text;

namespace RoboNet.EMVParser
{

    [Flags]
    public enum MemoryVariantGeneration
    {
        Span=1,
        Memory=2,
        All = Span | Memory
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class MemoryVariantMethodGeneratorAttribute : System.Attribute
    {
        public MemoryVariantGeneration GenerateSpanVariants { get; }

        public MemoryVariantMethodGeneratorAttribute(MemoryVariantGeneration generateSpanVariants = MemoryVariantGeneration.All)
        {
            GenerateSpanVariants = generateSpanVariants;
        }
    }
}", Encoding.UTF8)));

        initContext.RegisterSourceOutput(enumsToGenerate, (spc, nameAndContent) =>
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax)nameAndContent;

            var attribute = methodDeclarationSyntax.AttributeLists.FirstOrDefault(s => s.Attributes
                    .Any(a => a.Name.ToString() == "MemoryVariantMethodGenerator")).Attributes
                .FirstOrDefault(a => a.Name.ToString() == "MemoryVariantMethodGenerator");

            var generationMode = MemoryVariantGeneration.All;

            if (attribute?.ArgumentList != null)
            {
                var argument = attribute.ArgumentList.Arguments.FirstOrDefault();
                var memory = argument?.ToString(); //MemoryVariantGeneration.Memory;
                if (string.Equals(memory, "MemoryVariantGeneration.Memory", StringComparison.OrdinalIgnoreCase))
                    generationMode = MemoryVariantGeneration.Memory;
                else if (string.Equals(memory, "MemoryVariantGeneration.Span", StringComparison.OrdinalIgnoreCase))
                    generationMode = MemoryVariantGeneration.Span;
                else if (string.Equals(memory, "MemoryVariantGeneration.All", StringComparison.OrdinalIgnoreCase))
                    generationMode = MemoryVariantGeneration.All;
            }

            var methodString = methodDeclarationSyntax.ToString();

            var d = methodDeclarationSyntax.GetLeadingTrivia()
                .FirstOrDefault(s => s.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));

            var commentText = "///" + d.ToString();

            var readonlyTag = generationMode.HasFlag(MemoryVariantGeneration.Memory)
                ? methodString
                    .Replace("[ReadTagValueGeneration]", "")
                    .Replace("Memory<byte>", "ReadOnlyMemory<byte>")
                    .Replace("TagPointer", "TagPointerReadonly")
                    .Replace("IReadOnlyList<TagPointer>", "IReadOnlyList<TagPointerReadonly>")
                    .Replace("List<TagPointer>", "List<TagPointerReadonly>")
                    .Replace("new List<TagPointer>()", "new List<TagPointerReadonly>()")
                    .Replace("new TagPointer()", "new TagPointerReadonly()")
                    .Replace("TagLengthPointer", "TagLengthPointerReadonly")
                    .Replace("IReadOnlyList<TagLengthPointer>", "IReadOnlyList<TagLengthPointerReadonly>")
                    .Replace("List<TagLengthPointer>", "List<TagLengthPointerReadonly>")
                    .Replace("new List<TagLengthPointer>()", "new List<TagLengthPointerReadonly>()")
                    .Replace("new TagLengthPointer()", "new TagLengthPointerReadonly()")
                : "";

            var spanTag = generationMode.HasFlag(MemoryVariantGeneration.Span)
                ? methodString
                    .Replace("[ReadTagValueGeneration]", "")
                    .Replace(".Span", "")
                    .Replace("Memory<byte>", "Span<byte>")
                : "";

            var readonlySpanTag = spanTag
                .Replace("Span<byte>", "ReadOnlySpan<byte>")
                .Replace("TagPointer", "TagPointerReadonly")
                .Replace("IReadOnlyList<TagPointer>", "IReadOnlyList<TagPointerReadonly>")
                .Replace("List<TagPointer>", "List<TagPointerReadonly>")
                .Replace("new List<TagPointer>()", "new List<TagPointerReadonly>()")
                .Replace("new TagPointer()", "new TagPointerReadonly()")
                .Replace("TagLengthPointer", "TagLengthPointerReadonly")
                .Replace("IReadOnlyList<TagLengthPointer>", "IReadOnlyList<TagLengthPointerReadonly>")
                .Replace("List<TagLengthPointer>", "List<TagLengthPointerReadonly>")
                .Replace("new List<TagLengthPointer>()", "new List<TagLengthPointerReadonly>()")
                .Replace("new TagLengthPointer()", "new TagLengthPointerReadonly()");

            var className = (methodDeclarationSyntax.Parent as ClassDeclarationSyntax)?.Identifier.Text;
            
            var classText = $@"
using System.Text;

namespace RoboNet.EMVParser;

#nullable enable
public static partial class {className}
{{
    {(generationMode.HasFlag(MemoryVariantGeneration.Memory) ? commentText : "")}
    {readonlyTag}

    {(generationMode.HasFlag(MemoryVariantGeneration.Span) ? commentText : "")}
    {spanTag}

    {(generationMode.HasFlag(MemoryVariantGeneration.Span) ? commentText : "")}
    {readonlySpanTag}
}}
#nullable disable";

            spc.AddSource(
                $"{className}_{methodDeclarationSyntax.Identifier}_{methodDeclarationSyntax.ParameterList.ToString()
                    .Replace(" ", "")
                    .Replace(",", "")
                    .Replace("[", "").Replace("]", "")
                    .Replace("<", "").Replace(">", "")
                    .Replace("(", "").Replace(")", "")}.g.cs",
                SourceText.From(classText, Encoding.UTF8));
        });
    }

    [Flags]
    public enum MemoryVariantGeneration
    {
        Span = 1,
        Memory = 2,
        All = Span | Memory
    }
}