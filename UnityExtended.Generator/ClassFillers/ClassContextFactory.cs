using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using UnityExtended.Generator.FillerData;

namespace UnityExtended.Generator.ClassFillers;

public static class ClassContextFactory {
    public static Class? GetComponentFiller(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var fqClassName = context.TargetSymbol.ContainingType.ToDisplayString();
        var fieldSymbol = context.TargetSymbol as IFieldSymbol;

        if (fieldSymbol == null) return null;

        var filler = new GetComponentFiller();
        var data = new GetComponentFillerData(fqClassName, fieldSymbol, context.Attributes[0]);

        Class c = Class.GetOrCreate(data.FullyQualifiedGeneratedClassName);

        return filler.Fill(c, data);
    }
    
    public static Class? GetComponentAheadFiller(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var fqClassName = context.TargetSymbol.ContainingType.ToDisplayString();
        var fieldSymbol = context.TargetSymbol as IFieldSymbol;

        if (fieldSymbol == null) return null;

        var filler = new GetComponentAheadFiller();
        var data = new GetComponentAheadFillerData(fqClassName, fieldSymbol, context.Attributes[0]);

        Class c = Class.GetOrCreate(data.FullyQualifiedGeneratedClassName);

        return filler.Fill(c, data);
    }
}