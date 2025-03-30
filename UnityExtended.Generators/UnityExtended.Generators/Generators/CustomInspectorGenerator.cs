using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using UnityExtended.Generator;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

[Generator]
public class CustomInspectorGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // var createCustomInspectorProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
        //         AttributesHelper.CreateCustomInspectorAttribute,
        //         predicate: GeneratorHelper.TruePredicate,
        //         transform: FillerDataFactory.CreateCustomInspectorFillerDataFromContext)
        //     .Where(x => x.HasValue)
        //     .Select((x, _) => x!.Value as IFillerData)
        //     .Collect();
        //
        // context.RegisterSourceOutput(createCustomInspectorProvider, GenerateCode);
    }

    // private static void GenerateCode(SourceProductionContext context, ImmutableArray<IFillerData> data) {
    //     Class.ClearStaticState();
    //     List<CustomEditorClass> generatedClasses = new();
    //
    //     foreach (var dataByClassName in data.GroupBy(x => x.FullyQualifiedGeneratedClassName)) {
    //         CustomEditorClass c = CustomEditorClass.GetOrCreate(dataByClassName.Key);
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