using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnityExtended.Generator;

public static class GeneratorHelper {
    public const string AttributesNamespace = "UnityExtended.Generators.Attributes";

    public const string AwakeMethodSignature = "private void Awake()";
    public const string Awake2MethodSignature = "partial void Awake2()";
    
    public const string OnEnableMethodSignature = "private void OnEnable()";
    public const string OnEnable2MethodSignature = "partial void OnEnable2()";
    
    public const string OnDisableMethodSignature = "private void OnDisable()";
    public const string OnDisable2MethodSignature = "partial void OnDisable2()";

    public const string OnValidateMethodSignature = "private void OnValidate()";
    public const string OnValidate2MethodSignature = "partial void OnValidate2()";
    
    public const string OnDestroyMethodSignature = "private void OnDestroy()";
    public const string OnDestroy2MethodSignature = "partial void OnDestroy2()";

    public static readonly string[] InputActionPostfixes = ["Performed", "Started", "Canceled"];
    
    public static IEnumerable<Class> ExtractGeneratedClassesFromData(IEnumerable<IGenerate> requiredGeneratedData) {
        Dictionary<string, Class> classesToGenerate = new();
        
        foreach (var generateData in requiredGeneratedData) {
            if (generateData is IGenerateClass classGenerator) {
                Class generatedClass = classGenerator.GeneratedClass;
                
                if (classesToGenerate.TryGetValue(generatedClass.FullyQualifiedName,
                        out Class existing)) {
                    existing.Merge(generatedClass);
                }
                else {
                    classesToGenerate.Add(generatedClass.FullyQualifiedName, generatedClass);
                }
            }
        }

        return classesToGenerate.Values;
    }

    public static void AddClassesToSource(this SourceProductionContext context, IEnumerable<Class> classesToGenerate) {
        foreach (var classToGenerate in classesToGenerate) {
            IndentedStringBuilder stringBuilder = new();

            classToGenerate.AppendTo(stringBuilder);
            
            context.AddSource($"{classToGenerate.FullyQualifiedName}.g.cs", stringBuilder.ToString());
        }
    }
    
    public static IncrementalValueProvider<IEnumerable<T>> ValuesCombine<T>(
        params IncrementalValueProvider<ImmutableArray<T>>[] providers) {
        if (providers.Length == 0) return default;
        if (providers.Length == 1) return providers[1].AsEnumerable();

        var provider = providers[0].AsEnumerable();
        
        for (int i = 1; i < providers.Length; i++) {
            provider = provider.ValuesCombine(providers[i].AsEnumerable());
        }

        return provider;
    }
}