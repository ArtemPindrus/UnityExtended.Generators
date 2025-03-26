using System.Collections.Generic;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public readonly record struct CollectAttributeData : IGenerateClass {
    public const string PreMethodName = "PreCollect";
    
    public Class GeneratedClass { get; }

    private CollectAttributeData(ITypeSymbol classSymbol) {
        string fqClassName = classSymbol.ToDisplayString();
        
        GeneratedClass = Class.GetOrCreate(fqClassName)
            .AddFields($"private static System.Collections.Generic.Dictionary<int, {fqClassName}> collection;");

        GeneratedClass.GetOrCreateMethod($"public static bool TryGetByHashCode(int hashCode, out {fqClassName} item)")
            .AddStatements("return collection.TryGetValue(hashCode, out item);");
        
        GeneratedClass.GetOrCreateMethod("private static void Init()")
            .AddAttribute("[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]")
            .AddStatements("collection = new();");
        
        GeneratedClass.GetOrCreateMethod("private void Awake()")
            .AddStatements($"""
                            {PreMethodName}();
                            collection.Add(GetHashCode(), this);
                            """);

        GeneratedClass.GetOrCreateMethod("private void OnDestroy()")
            .AddStatements("collection.Remove(GetHashCode());");

        GeneratedClass.GetOrCreateMethod($"partial void {PreMethodName}()");
    }

    public static IGenerate TransformIntoIGenerate(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var classSymbol = (ITypeSymbol)context.TargetSymbol;

        return new CollectAttributeData(classSymbol);
    }
}