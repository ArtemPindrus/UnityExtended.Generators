using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnityExtended.Generator;

[Generator]
public class UnityGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var getComponentProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{GeneratorHelper.AttributesNamespace}.GetComponentAttribute",
                static (_,_) => true, GetComponentAttributeData.Transform)
            .WhereNotNullValues()
            .Collect()
            .Select((x, _) => GetComponentAttributeData.Assemble(x));
        
        var getComponentAheadProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{GeneratorHelper.AttributesNamespace}.GetComponentAheadAttribute",
                static (_,_) => true, GetComponentAheadAttributeData.Transform)
            .WhereNotNullValues()
            .Collect()
            .Select((x, _) => GetComponentAheadAttributeData.Assemble(x));
        
        var handleInputProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{GeneratorHelper.AttributesNamespace}.HandleInputAttribute",
                static (_,_) => true, HandleInputAttributeData.TransformToIGenerate)
            .WhereNotNullValues()
            .Collect();
        
        var collectProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{GeneratorHelper.AttributesNamespace}.CollectAttribute",
                static (_,_) => true, CollectAttributeData.TransformIntoIGenerate)
            .Collect();
        
        var serializePropertyProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{GeneratorHelper.AttributesNamespace}.SerializePropertyWithBackingAttribute",
            static (_,_) => true, SerializePropertyWithBackingAttributeData.TransformIntoIGenerate)
            .Collect();

        var foldoutGroupProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{GeneratorHelper.AttributesNamespace}.StartFoldoutGroupAttribute",
            static (_, _) => true, FoldoutGroupAttributeData.TransformIntoIGenerate)
            .Collect();
        
        var displayProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (n,_) => n is ClassDeclarationSyntax,
                transform: DisplayAttributeData.Transform)
            .WhereNotNullValues()
            .Collect();

        var provider = GeneratorHelper.ValuesCombine(getComponentProvider, 
            getComponentAheadProvider,
            handleInputProvider, 
            collectProvider, 
            serializePropertyProvider, 
            foldoutGroupProvider,
            displayProvider);
        
        context.RegisterSourceOutput(provider, Execute);
    }
    
    private static void Execute(SourceProductionContext context, ImmutableArray<IGenerate> requiredGeneratedData) {
        var classes = GeneratorHelper.ExtractGeneratedClassesFromData(requiredGeneratedData);

        AddSecondMethodsIfNecessary(classes);
        
        context.AddSource(classes);
    }
    
    private static void AddSecondMethodsIfNecessary(IEnumerable<Class> classesToGenerate) {
        foreach (var classToGenerate in classesToGenerate) {
            Add(classToGenerate, GeneratorHelper.AwakeMethodSignature, GeneratorHelper.Awake2MethodSignature);
            
            Add(classToGenerate, GeneratorHelper.OnEnableMethodSignature, GeneratorHelper.OnEnable2MethodSignature);
            
            Add(classToGenerate, GeneratorHelper.OnDisableMethodSignature, GeneratorHelper.OnDisable2MethodSignature);
            
            Add(classToGenerate, GeneratorHelper.OnValidateMethodSignature, GeneratorHelper.OnValidate2MethodSignature);
            
            Add(classToGenerate, GeneratorHelper.OnDestroyMethodSignature, GeneratorHelper.OnDestroy2MethodSignature);
        }

        static void Add(Class classToGenerate, string existingSignature, string generatedSignature, bool addCallAtTheEnd = true) {
            Method? existingMethod =
                classToGenerate.Methods.FirstOrDefault(x => x.Signature == existingSignature);
            
            if (existingMethod != default) {
                var generateMethod = new Method(generatedSignature);
                classToGenerate.AddMethod(generateMethod);

                if (addCallAtTheEnd) {
                    var calling = generatedSignature.Substring(generatedSignature.LastIndexOf(' ') + 1) + ";";

                    existingMethod.AddStatement(calling);
                }
            }
        }
    }
}