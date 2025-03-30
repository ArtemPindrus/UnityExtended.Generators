using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

[Generator]
public class GetComponentGenerator : IIncrementalGenerator {
    private static readonly GetComponentFiller GetComponentFiller = new();

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var getComponentProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributesHelper.GetComponentAttribute,
                predicate: GeneratorHelper.TruePredicate,
                transform: FillerDataFactory.GetComponentFillerDataFromContext)
            .Where(x => x.HasValue)
            .Select((x, _) => x!.Value)
            .Collect();
        
        // TODO: Changing any GetComponent triggers regeneration of all of them. Change?
        context.RegisterSourceOutput(getComponentProvider, Action);
    }

    private static void Action(SourceProductionContext context, ImmutableArray<GetComponentFillerData> data) {
        foreach (var groupedByFQName in data.GroupBy(x => x.FullyQualifiedGeneratedClassName)) {
            LoggingHelper.Log($"Generating for {groupedByFQName.Key}.", "GetComponentAction");
            
            Class c = new Class(groupedByFQName.Key);
            
            foreach (var getComponentFillerData in groupedByFQName) {
                ClassFillerUtility.Fill(c, GetComponentFiller, getComponentFillerData);
            }
            
            context.AddSource(c, "GetComponent");
        }
    }
}