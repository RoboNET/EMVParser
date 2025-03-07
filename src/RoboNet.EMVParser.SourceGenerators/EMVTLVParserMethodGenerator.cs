using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoboNet.EMVParser.SourceGenerators;

[Generator]
public class EMVTLVParserMethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var enumsToGenerate = initContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                "RoboNet.EMVParser.ReadTagValueGenerationAttribute",
                predicate: (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) => ctx.TargetNode);


        initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "ReadTagValueGenerationAttribute.g.cs",
            SourceText.From(@"
namespace RoboNet.EMVParser
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ReadTagValueGenerationAttribute : System.Attribute
    {
    }
}", Encoding.UTF8)));

        initContext.RegisterSourceOutput(enumsToGenerate, (spc, nameAndContent) =>
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax)nameAndContent;

            var methodString = methodDeclarationSyntax.ToString();

            var d = methodDeclarationSyntax.GetLeadingTrivia()
                .FirstOrDefault(s => s.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));

            var commentText = "///" + d.ToString();

            var readonlyTag = methodString
                .Replace("[ReadTagValueGeneration]", "")
                .Replace("Memory<byte>", "ReadOnlyMemory<byte>")
                .Replace("TagPointer", "TagPointerReadonly")
                .Replace("IReadOnlyList<TagPointer>", "IReadOnlyList<TagPointerReadonly>")
                .Replace("List<TagPointer>", "List<TagPointerReadonly>")
                .Replace("new List<TagPointer>()", "new List<TagPointerReadonly>()")
                .Replace("new TagPointer()", "new TagPointerReadonly()");

            var spanTag = methodString
                .Replace("[ReadTagValueGeneration]", "")
                .Replace(".Span", "")
                .Replace("Memory<byte>", "Span<byte>");

            var readonlySpanTag = spanTag
                .Replace("Span<byte>", "ReadOnlySpan<byte>")
                .Replace("TagPointer", "TagPointerReadonly")
                .Replace("IReadOnlyList<TagPointer>", "IReadOnlyList<TagPointerReadonly>")
                .Replace("List<TagPointer>", "List<TagPointerReadonly>")
                .Replace("new List<TagPointer>()", "new List<TagPointerReadonly>()")
                .Replace("new TagPointer()", "new TagPointerReadonly()");

            var classText = $@"
namespace RoboNet.EMVParser;

public static partial class EMVTLVParser
{{
    {commentText}
    {readonlyTag}

    {commentText}
    {spanTag}

    {commentText}
    {readonlySpanTag}
}}";


            spc.AddSource(
                $"EMVTLVParser_{methodDeclarationSyntax.ParameterList.ToString()
                    .Replace(" ", "")
                    .Replace(",", "")
                    .Replace("[", "").Replace("]", "")
                    .Replace("<", "").Replace(">", "")
                    .Replace("(", "").Replace(")", "")}.g.cs",
                SourceText.From(classText, Encoding.UTF8));
        });
    }
}