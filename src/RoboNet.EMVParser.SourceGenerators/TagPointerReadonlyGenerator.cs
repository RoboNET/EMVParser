using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoboNet.EMVParser.SourceGenerators;

[Generator]
public class TagPointerReadonlyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var enumsToGenerate = initContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                "RoboNet.EMVParser.TagPointerGenerationAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => ctx.SemanticModel);
        
        initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "TagPointerGenerationAttribute.g.cs",
            SourceText.From(@"
namespace RoboNet.EMVParser
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class TagPointerGenerationAttribute : System.Attribute
    {
    }
}", Encoding.UTF8)));

        initContext.RegisterSourceOutput(enumsToGenerate, (spc, nameAndContent) =>
        {
            var readonlyTag = nameAndContent.SyntaxTree.ToString()
                .Replace("[TagPointerGeneration]","")
                .Replace("TagPointer", "TagPointerReadonly")
                .Replace("Memory<byte>", "ReadOnlyMemory<byte>");

            spc.AddSource($"TagPointerReadonly.g.cs", SourceText.From(readonlyTag, Encoding.UTF8));
        });
    }
}

[Generator]
public class TagLengthPointerReadonlyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var enumsToGenerate = initContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                "RoboNet.EMVParser.TagLengthPointerGenerationAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => ctx.SemanticModel);
        
        initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "TagLengthPointerGenerationAttribute.g.cs",
            SourceText.From(@"
namespace RoboNet.EMVParser
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class TagLengthPointerGenerationAttribute : System.Attribute
    {
    }
}", Encoding.UTF8)));

        initContext.RegisterSourceOutput(enumsToGenerate, (spc, nameAndContent) =>
        {
            var readonlyTag = nameAndContent.SyntaxTree.ToString()
                .Replace("[TagLengthPointerGeneration]","")
                .Replace("TagLengthPointer", "TagLengthPointerReadonly")
                .Replace("Memory<byte>", "ReadOnlyMemory<byte>");

            spc.AddSource($"TagLengthPointerReadonly.g.cs", SourceText.From(readonlyTag, Encoding.UTF8));
        });
    }
}