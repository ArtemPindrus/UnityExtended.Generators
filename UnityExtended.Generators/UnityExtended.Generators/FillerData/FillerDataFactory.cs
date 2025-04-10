using System.Linq;
using System.Numerics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityExtended.Generator;
using UnityExtended.Generators.Extensions;
using UnityExtended.Generators.FillerData.Helpers;

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

    public static CollectFillerData CollectFillerDataFromContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        INamedTypeSymbol classSymbol = (INamedTypeSymbol)context.TargetSymbol;

        return new CollectFillerData(classSymbol.ToDisplayString());
    }

    public static CreateCustomInspectorFillerData? CreateCustomInspectorFillerDataFromContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol) return null;

        return new CreateCustomInspectorFillerData(classSymbol.ToDisplayString());
    }
}