using System.Threading;
using Microsoft.CodeAnalysis;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

public class CollectGenerator : FunctionalGenerator<CollectFillerData> {
    private static readonly CollectFiller Filler = new();


    protected override IncrementalValuesProvider<CollectFillerData> CreatePipeline(IncrementalGeneratorInitializationContext context) {
        var collectProvider = context.SyntaxProvider.ForAttributeWithMetadataName(AttributesHelper.CollectAttribute,
            predicate: GeneratorHelper.TruePredicate,
            transform: CollectFillerDataFromContext);

        return collectProvider;
    }
    
    protected override void Action(SourceProductionContext context, CollectFillerData data) {
        LoggingHelper.Log($"Generating for {data.FullyQualifiedGeneratedClassName}.", "collect");
        
        Class c = new(data.FullyQualifiedGeneratedClassName);

        Filler.Fill(c, data);
        
        context.AddSource(c, "Collect");
    }
    
    private static CollectFillerData CollectFillerDataFromContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        INamedTypeSymbol classSymbol = (INamedTypeSymbol)context.TargetSymbol;

        return new CollectFillerData(classSymbol.ToDisplayString());
    }
}
