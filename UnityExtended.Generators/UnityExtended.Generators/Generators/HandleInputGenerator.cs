using Microsoft.CodeAnalysis;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

[Generator]
public class HandleInputGenerator : IIncrementalGenerator {
    private static readonly HandleInputFiller filler = new();
    
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var handleInputProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributesHelper.HandleInputAttribute,
                predicate: GeneratorHelper.TruePredicate,
                transform: FillerDataFactory.HandleInputFillerDataFromContext)
            .Where(x => x.HasValue)
            .Select((x, _) => x!.Value);
        
        context.RegisterSourceOutput(handleInputProvider, Action);
    }

    private static void Action(SourceProductionContext context, HandleInputFillerData data) {
        LoggingHelper.Log($"Generating for {data.FullyQualifiedGeneratedClassName}.", "handleinput");
        
        Class c = new(data.FullyQualifiedGeneratedClassName);
        
        ClassFillerUtility.Fill(c, filler, data);
        
        context.AddSource($"{data.FullyQualifiedGeneratedClassName}_HandleInput.cs", c.ToString());
    }
}