using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

[Generator]
public class UnityGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // TODO: ordering regeneration problem on all providers
        
        GetComponentGenerator getComponentGenerator = new();
        var getComponentProvider = getComponentGenerator.Initialize(context);
        
        var handleInputProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributesHelper.HandleInputAttribute,
                predicate: GeneratorHelper.TruePredicate,
                transform: FillerDataFactory.HandleInputFillerDataFromContext)
            .Where(x => x.HasValue)
            .Select((x, _) => x!.Value);
        
        context.RegisterSourceOutput(handleInputProvider, HandleInputGenerator.Action);
        
        // collect
        var collectProvider = context.SyntaxProvider.ForAttributeWithMetadataName(AttributesHelper.CollectAttribute,
            predicate: GeneratorHelper.TruePredicate,
            transform: FillerDataFactory.CollectFillerDataFromContext);
        
        context.RegisterSourceOutput(collectProvider, CollectGenerator.Action);
        
        // MonoBehaviourHook
        var combined = getComponentProvider
            .Collect()
            .Combine(handleInputProvider.Collect())
            .Combine(collectProvider.Collect());
        
        var hookProvider = combined.SelectMany((t, _) => {
            var ((getComponentData, handleInputData), collectData) = t;
        
            Dictionary<string, MonoBehaviourHook> nameToHook = new(); // name of class to Hook
            
            foreach (var d in getComponentData) {
                TryFindAndModifyOrCreate(d.FullyQualifiedGeneratedClassName, 
                    h => h with { HookGetComponent = true }, 
                    () => new MonoBehaviourHook(d.FullyQualifiedGeneratedClassName) { HookGetComponent = true});
            }
        
            foreach (var d in handleInputData) {
                TryFindAndModifyOrCreate(d.FullyQualifiedGeneratedClassName,
                    h => h with { HookHandleInput = true },
                    () => new MonoBehaviourHook(d.FullyQualifiedGeneratedClassName) { HookHandleInput = true });
            }
        
            foreach (var d in collectData) {
                TryFindAndModifyOrCreate(d.FullyQualifiedGeneratedClassName,
                    h => h with { HookCollect = true },
                    () => new MonoBehaviourHook(d.FullyQualifiedGeneratedClassName) { HookCollect = true });
            }
        
            return nameToHook.Values;
        
            void TryFindAndModifyOrCreate(string fqClassName, Func<MonoBehaviourHook, MonoBehaviourHook> modify, Func<MonoBehaviourHook> create) {
                if (nameToHook.TryGetValue(fqClassName, out var hook)) {
                    nameToHook[fqClassName] = modify(hook);
                }
                else nameToHook[fqClassName] = create();
            }
        });
        
        context.RegisterSourceOutput(hookProvider, MonoBehaviourHookGenerator.Action);
    }
}