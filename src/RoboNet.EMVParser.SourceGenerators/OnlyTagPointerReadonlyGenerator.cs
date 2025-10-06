using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoboNet.EMVParser.SourceGenerators;

[Generator]
public class OnlyTagPointerReadonlyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var enumsToGenerate = initContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                "RoboNet.EMVParser.OnlyTagPointerGenerationAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => ctx.SemanticModel);
        
        initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "OnlyTagPointerGenerationAttribute.g.cs",
            SourceText.From(@"
namespace RoboNet.EMVParser
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class OnlyTagPointerGenerationAttribute : System.Attribute
    {
    }
}", Encoding.UTF8)));

        initContext.RegisterSourceOutput(enumsToGenerate, (spc, nameAndContent) =>
        {
            var readonlyTag = nameAndContent.SyntaxTree.ToString()
                .Replace("[OnlyTagPointerGeneration]","")
                .Replace("OnlyTagPointer", "OnlyTagPointerReadonly")
                .Replace("Memory<byte>", "ReadOnlyMemory<byte>");

            readonlyTag = $@"#nullable enable
{readonlyTag}
#nullable disable";
            
            spc.AddSource($"OnlyTagPointerReadonly.g.cs", SourceText.From(readonlyTag, Encoding.UTF8));
        });
    }
}