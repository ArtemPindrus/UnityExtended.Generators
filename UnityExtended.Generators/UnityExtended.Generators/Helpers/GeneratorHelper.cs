using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UnityExtended.Generators.Extensions;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Helpers;

public static class GeneratorHelper {
    public const string GenerationPostfix = "_Generated";
    
    public const string AwakeMethodSignature = "protected void Awake()";
    public const string Awake2MethodSignature = "partial void Awake2()";
    
    public const string StartMethodSignature = "protected void Start()";
    public const string Start2MethodSignature = "partial void Start2()";
    
    public const string OnEnableMethodSignature = "protected void OnEnable()";
    public const string OnEnable2MethodSignature = "partial void OnEnable2()";
    
    public const string OnDisableMethodSignature = "protected void OnDisable()";
    public const string OnDisable2MethodSignature = "partial void OnDisable2()";

    public const string OnValidateMethodSignature = "protected void OnValidate()";
    public const string OnValidate2MethodSignature = "partial void OnValidate2()";
    
    public const string OnDestroyMethodSignature = "protected void OnDestroy()";
    public const string OnDestroy2MethodSignature = "partial void OnDestroy2()";

    public const string UpdateSignature = "protected void Update()";
    public const string Update2Signature = "partial void Update2()";
    
    public const string CreateInspectorGUISignature = "public override VisualElement CreateInspectorGUI()";

    
    public static readonly string[] InputActionPostfixes = ["Performed", "Started", "Canceled"];

    public static bool TruePredicate(SyntaxNode sn, CancellationToken ct) => true;

    public static void AddSource(this SourceProductionContext context, Class c, string fileNamePostfix) {
        context.AddSource($"{c.FullyQualifiedName}_{fileNamePostfix}.cs", c.ToString());
    }

    public static void AddSource(this SourceProductionContext context, IEnumerable<Class> classesToGenerate) {
        foreach (var classToGenerate in classesToGenerate) {
            IndentedStringBuilder stringBuilder = new();

            classToGenerate.AppendTo(stringBuilder);
            
            context.AddSource($"{classToGenerate.FullyQualifiedName}.g.cs", stringBuilder.ToString());
        }
    }
    
    public static IncrementalValueProvider<ImmutableArray<T>> ValuesCombine<T>(
        params IncrementalValueProvider<ImmutableArray<T>>[] providers) {
        if (providers.Length == 0) return default;
        if (providers.Length == 1) return providers[1];

        var provider = providers[0];
        
        for (int i = 1; i < providers.Length; i++) {
            provider = provider.ValuesCombine(providers[i]);
        }

        return provider;
    }
}