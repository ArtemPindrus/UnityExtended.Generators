using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

[Generator]
public class UnityGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // var getComponentProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
        //     AttributesHelper.GetComponentAttribute,
        //     predicate: GeneratorHelper.TruePredicate,
        //     transform: FillerDataFactory.GetComponentFillerDataFromContext)
        //     .Where(x => x.HasValue)
        //     .Select((x, _) => x!.Value as IFillerData)
        //     .Collect();
        //
        // var getComponentAheadProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
        //     AttributesHelper.GetComponentAheadAttribute,
        //     predicate: GeneratorHelper.TruePredicate,
        //     transform: FillerDataFactory.GetComponentAheadFillerDataFromContext)
        //     .Where(x => x.HasValue)
        //     .Select((x, _) => x!.Value as IFillerData)
        //     .Collect();
        //
        // var handleInputProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
        //         AttributesHelper.HandleInputAttribute,
        //         predicate: GeneratorHelper.TruePredicate,
        //         transform: FillerDataFactory.HandleInputFillerDataFromContext)
        //     .Where(x => x.HasValue)
        //     .Select((x, _) => x!.Value as IFillerData)
        //     .Collect();
        //
        // var collectFiller = context.SyntaxProvider.ForAttributeWithMetadataName(
        //         AttributesHelper.CollectAttribute,
        //         predicate: GeneratorHelper.TruePredicate,
        //         transform: FillerDataFactory.CollectFillerDataFromContext)
        //     .Where(x => x.HasValue)
        //     .Select((x, _) => x!.Value as IFillerData)
        //     .Collect();
        //
        // var provider = GeneratorHelper.ValuesCombine(getComponentProvider, 
        //     getComponentAheadProvider, 
        //     handleInputProvider,
        //     collectFiller);
        //
        // context.RegisterSourceOutput(provider, GenerateCode);
    }

    // private static void GenerateCode(SourceProductionContext context, ImmutableArray<IFillerData> data) {
    //     Class.ClearStaticState();
    //     List<Class> generatedClasses = new();
    //
    //     foreach (var dataByClassName in data.GroupBy(x => x.FullyQualifiedGeneratedClassName)) {
    //         Class c = Class.GetOrCreate(dataByClassName.Key);
    //         generatedClasses.Add(c);
    //         
    //         foreach (var fillerData in dataByClassName) {
    //             ClassFillerUtility.GenericFill(c, fillerData);
    //         }
    //
    //         c.Finish();
    //     }
    //     
    //     context.AddSource(generatedClasses);
    // }
}