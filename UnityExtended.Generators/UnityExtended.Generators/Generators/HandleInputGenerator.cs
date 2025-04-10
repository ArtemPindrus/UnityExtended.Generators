using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.Extensions;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

public class HandleInputGenerator : FunctionalGenerator<HandleInputFillerData> {
    private static readonly HandleInputFiller HandleInputFiller = new();

    protected override IncrementalValuesProvider<HandleInputFillerData> CreatePipeline(IncrementalGeneratorInitializationContext context) {
        var handleInputProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributesHelper.HandleInputAttribute,
                predicate: GeneratorHelper.TruePredicate,
                transform: HandleInputFillerDataFromContext)
            .Where(x => x.HasValue)
            .Select((x, _) => x!.Value);

        return handleInputProvider;
    }

    protected override void Action(SourceProductionContext context, HandleInputFillerData entry) {
        LoggingHelper.Log($"Generating for {entry.FullyQualifiedGeneratedClassName}.", "handleinput");
        
        Class c = new(entry.FullyQualifiedGeneratedClassName);

        HandleInputFiller.Fill(c, entry);
        
        context.AddSource($"{entry.FullyQualifiedGeneratedClassName}_HandleInput.cs", c.ToString());
    }
    
    public static HandleInputFillerData? HandleInputFillerDataFromContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        var classSymbol = context.TargetSymbol as INamedTypeSymbol;

        if (classSymbol == null) return null;
        
        var implementedClassMethods = classSymbol
            .GetMembers()
            .Where(x => x.Kind == SymbolKind.Method)
            .Select(x => (IMethodSymbol)x)
            .ToArray();

        var attributes = context.Attributes;

        EquatableList<InputAsset> assets = new();
        foreach (var attribute in attributes) {
            if (token.IsCancellationRequested) return null;
            
            attribute.GetParamValueAt(0, out INamedTypeSymbol? inputAssetType);
            
            if (inputAssetType == null) continue;
        
            var inputAsset = new InputAsset(inputAssetType.ToDisplayString(), inputAssetType.Name);

            var mapTypes = inputAssetType
                .GetTypeMembers()
                .Where(x => x.TypeKind == TypeKind.Struct && x.Name.Contains("Actions"));

            foreach (var mapType in mapTypes) {
                if (token.IsCancellationRequested) return null;
            
                ActionMap map = new(mapType.ToDisplayString(), mapType.Name);
            
                var actionProperties = mapType
                    .GetMembers()
                    .Where(x => x.Kind == SymbolKind.Property)
                    .Select(x => (IPropertySymbol)x)
                    .Where(x => x.Type.Name == "InputAction");

                foreach (var actionProperty in actionProperties) {
                    if (token.IsCancellationRequested) return null;

                    Action action = new(actionProperty.Name);

                    foreach (var @event in action.Events) {
                        if (token.IsCancellationRequested) return null;
                    
                        if (implementedClassMethods.Any(x => x.Name == @event.MethodName)) @event.SetIsImplemented();
                    }
                
                    map.Actions.Add(action);
                }
            
                inputAsset.ActionMaps.Add(map);
            }
            
            assets.Add(inputAsset);
        }

        return new HandleInputFillerData(classSymbol.ToDisplayString(), assets);
    }
}