using Microsoft.CodeAnalysis;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

[Generator]
public class CollectGenerator : IIncrementalGenerator {
    private static readonly CollectFiller filler = new();
    
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(AttributesHelper.CollectAttribute,
            predicate: GeneratorHelper.TruePredicate,
            transform: FillerDataFactory.CollectFillerDataFromContext);
        
        context.RegisterSourceOutput(provider, Action);
    }

    private static void Action(SourceProductionContext context, CollectFillerData data) {
        LoggingHelper.Log($"Generating for {data.FullyQualifiedGeneratedClassName}.", "collect");
        
        Class c = new(data.FullyQualifiedGeneratedClassName);

        ClassFillerUtility.Fill(c, filler, data);
        
        context.AddSource(c, "Collect");
    }
}