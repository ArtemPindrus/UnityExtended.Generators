using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using UnityExtended.Generator;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

[Generator]
public class CustomInspectorGenerator : IIncrementalGenerator {
    private readonly CreateCustomInspectorFiller filler = new();
    

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributesHelper.CreateCustomInspectorAttribute,
                predicate: GeneratorHelper.TruePredicate,
                transform: CreateCustomInspectorFillerDataFromContext)
            .Where(x => x.HasValue)
            .Select((x, _) => x!.Value);
        
        context.RegisterSourceOutput(provider, Action);
    }
    
    private void Action(SourceProductionContext context, CreateCustomInspectorFillerData data) {
        Class c = new(data.FullyQualifiedGeneratedClassName);

        filler.Fill(c, data);
        
        context.AddSource($"{data.FullyQualifiedGeneratedClassName}{GeneratorHelper.GenerationPostfix}.cs", c.ToString());
    }
    
    private static CreateCustomInspectorFillerData? CreateCustomInspectorFillerDataFromContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol) return null;

        return new CreateCustomInspectorFillerData(classSymbol.ToDisplayString());
    }
}
