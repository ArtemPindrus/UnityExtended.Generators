using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityExtended.Generators.Extensions;

namespace UnityExtended.Generators.FillerData;

public static class FillerDataFactory {
    public static GetComponentFillerData? GetComponentFillerDataFromContext(GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        var fqClassName = context.TargetSymbol.ContainingType.ToDisplayString();

        if (context.TargetSymbol is not IFieldSymbol fieldSymbol) return null;

        var data = new GetComponentFillerData(fqClassName, fieldSymbol, context.Attributes[0]);

        return data;
    }

    public static GetComponentAheadFillerData? GetComponentAheadFillerDataFromContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        var fqClassName = context.TargetSymbol.ContainingType.ToDisplayString();

        if (context.TargetSymbol is not IFieldSymbol fieldSymbol) return null;

        var data = new GetComponentAheadFillerData(fqClassName, fieldSymbol, context.Attributes[0]);

        return data;
    }

    public static HandleInputFillerData? HandleInputFillerDataFromContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        var classSymbol = context.TargetSymbol as INamedTypeSymbol;

        if (classSymbol == null) return null;

        var attribute = context.Attributes[0];
        attribute.GetParamValueAt(0, out INamedTypeSymbol? inputAssetType);

        if (inputAssetType == null) return null;

        var implementedClassMethods = classSymbol
            .GetMembers()
            .Where(x => x.Kind == SymbolKind.Method)
            .Select(x => (IMethodSymbol)x)
            .ToArray();
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

        return new HandleInputFillerData(classSymbol.ToDisplayString(), inputAsset);
    }
}