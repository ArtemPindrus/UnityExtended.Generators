using System.Collections.Generic;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public readonly record struct CollectAttributeData : IGenerateClass {
    public const string PreMethodName = "PreCollect";
    
    public Class GeneratedClass { get; }

    private CollectAttributeData(ITypeSymbol classSymbol) {
        GeneratedClass = new Class(classSymbol.ToDisplayString());
        string fqClassName = GeneratedClass.FullyQualifiedName;

        GeneratedClass.AddField($"private static System.Collections.Generic.Dictionary<int, {fqClassName}> collection;");

        Method preMethod = new($"partial void {PreMethodName}()");

        Method tryGetByHashCode = new($"public static bool TryGetByHashCode(int hashCode, out {fqClassName} item)");
        tryGetByHashCode.AddStatement("return collection.TryGetValue(hashCode, out item);");

        Method init = new("""
                          private static void Init()
                          """);
        init.AddAttribute("[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]");
        init.AddStatement("collection = new();");

        Method awake = new("private void Awake()");
        awake.AddStatement($"{PreMethodName}();");
        awake.AddStatement("collection.Add(GetHashCode(), this);");

        Method onDestroy = new("private void OnDestroy()");
        onDestroy.AddStatement("collection.Remove(GetHashCode());");
        
        GeneratedClass.AddMethods(tryGetByHashCode, init, awake, onDestroy, preMethod);
    }

    public static IGenerate TransformIntoIGenerate(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var classSymbol = (ITypeSymbol)context.TargetSymbol;

        return new CollectAttributeData(classSymbol);
    }
}