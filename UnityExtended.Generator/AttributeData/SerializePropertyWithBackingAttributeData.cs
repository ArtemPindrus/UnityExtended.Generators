using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityExtended.Generator;

public readonly record struct SerializePropertyWithBackingAttributeData : IGenerateClass {
    public Class GeneratedClass { get; }

    private SerializePropertyWithBackingAttributeData(SemanticModel semanticModel, ITypeSymbol classSymbol, PropertyDeclarationSyntax propertyDeclarationSyntax) {
        GeneratedClass = new(classSymbol.ToDisplayString());

        Method onValidate = new(GeneratorHelper.OnValidateMethodSignature);

        var type = propertyDeclarationSyntax.Type;
        var typeSymbol = semanticModel.GetSymbolInfo(type).Symbol;
        var typeName = typeSymbol.ToDisplayString();

        var identifier = propertyDeclarationSyntax.Identifier.ValueText;
        var fieldName = identifier.ToLowerFirst();
        
        GeneratedClass.AddField($"[UnityEngine.SerializeField] private {typeName} {fieldName};");
        onValidate.AddStatement($"{identifier} = {fieldName};");
        
        GeneratedClass.AddMethod(onValidate);
    }

    public static IGenerate TransformIntoIGenerate(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        SemanticModel semanticModel = context.SemanticModel;

        var classSymbol = (ITypeSymbol)context.TargetSymbol.ContainingSymbol;

        return new SerializePropertyWithBackingAttributeData(semanticModel, classSymbol, (PropertyDeclarationSyntax)context.TargetNode);
    }
}