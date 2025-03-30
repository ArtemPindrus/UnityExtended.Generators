using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public class CollectFiller : IClassFiller<CollectFillerData, Class> {
    private const string CollectionFieldName = "collection";
    public const string MethodSignature = $"private void Collect{GeneratorHelper.GenerationPostfix}()";
    
    public Class Fill(Class c, CollectFillerData data) {
        c.AddAttribute("#nullable enable");
        
        c.AddUsings($"""
                    System
                    System.Collections.Generic
                    UnityEngine
                    """);
        
        c.AddFields($"""
                    private static Dictionary<int, {data.FullyQualifiedGeneratedClassName}> {CollectionFieldName};
                    public event Action<{data.FullyQualifiedGeneratedClassName}>? OnCreated;
                    """);

        var initCollectionMethod = c.GetOrCreateMethod("private static void InitializeCollection()")
            .AddAttribute("[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]")
            .AddStatement($"{CollectionFieldName} = new();");

        var getByHashMethod = c
            .GetOrCreateMethod($"public static bool TryGetInstanceByHashCode(int hashCode, out {data.FullyQualifiedGeneratedClassName}? instance)")
            .AddStatement($"return {CollectionFieldName}.TryGetValue(hashCode, out instance);");

        var startMethod = c.GetOrCreateMethod(MethodSignature);
        
        startMethod.AddStatements($"""
                                   OnCreated?.Invoke(this);
                                   {CollectionFieldName}.Add(GetHashCode(), this);
                                   """);

        return c;
    }
}