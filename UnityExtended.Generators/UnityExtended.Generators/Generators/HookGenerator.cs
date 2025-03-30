using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityExtended.Generators.ClassFillers;
using UnityExtended.Generators.Extensions;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.Generators;

[Generator]
public class HookGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(AttributesHelper.GeneratorHookAttribute,
            predicate: GeneratorHelper.TruePredicate,
            transform: Transform)
            .Where(x => x.HasValue)
            .Select((x, _) => x!.Value);
        
        context.RegisterSourceOutput(provider, Action);
    }

    private static Hook? Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol 
            || !classSymbol.IsDerivedFrom("UnityEngine.MonoBehaviour")) return null;
        
        var classFQName = classSymbol.ToDisplayString();
        
        var attribute = context.Attributes[0];
        var attributeArguments = attribute.ConstructorArguments;

        var members = classSymbol.GetMembers();

        var fields = members.Where(x => x.Kind == SymbolKind.Field).Select(x => (IFieldSymbol)x);

        bool handleInputPresent = classSymbol.GetAttribute(AttributesHelper.HandleInputAttribute, out _);
        bool collectPresent = classSymbol.GetAttribute(AttributesHelper.CollectAttribute, out _);
        bool getComponentPresent = fields.Any(x => x.GetAttribute(AttributesHelper.GetComponentAttribute, out var data));

        return new Hook(classFQName,
            HookGetComponent: getComponentPresent,
            HookHandleInput: handleInputPresent,
            HookCollect: collectPresent,
            CallBase: (bool)attributeArguments[0].Value);
    }
    
    private static void Action(SourceProductionContext context, Hook hook) {
        LoggingHelper.Log($"Generating for {hook.FullyQualifiedClassName}.", "Hook");
        
        Class c = new Class(hook.FullyQualifiedClassName);
        
        CreateStartHook(c, hook);
        CreateAwakeHook(c, hook);
        CreateOnEnableHook(c, hook);
        CreateOnDisableHook(c, hook);
        
        context.AddSource($"{hook.FullyQualifiedClassName}_Hook.cs", c.ToString());
    }

    private static void CreateStartHook(Class c, Hook hook) {
        var startMethod = c.GetOrCreateMethod(GeneratorHelper.StartMethodSignature);
        
        if (hook.CallBase) startMethod.AddStatement("base.Start();");

        if (hook.HookCollect) {
            c.EnsureMethodIsCreatedAndCallIt("partial void PreCollect()", startMethod);
            startMethod.AddStatement(HierarchyHelper.MethodSignatureIntoCallStatement(CollectFiller.MethodSignature));
            c.EnsureMethodIsCreatedAndCallIt("partial void PostCollect()", startMethod);
        }
        
        c.EnsureMethodIsCreatedAndCallIt(GeneratorHelper.Start2MethodSignature, startMethod);
    }
    
    private static void CreateAwakeHook(Class c, Hook hook) {
        var awakeMethod = c.GetOrCreateMethod(GeneratorHelper.AwakeMethodSignature);
        
        if (hook.CallBase) awakeMethod.AddStatement("base.Awake();");

        if (hook.HookGetComponent) {
            c.EnsureMethodIsCreatedAndCallIt("partial void PreGetComponent()", awakeMethod);
            awakeMethod.AddStatement($"{GetComponentFiller.MethodName}();");
            c.EnsureMethodIsCreatedAndCallIt("partial void PostGetComponent()", awakeMethod);
        }

        if (hook.HookHandleInput) {
            c.EnsureMethodIsCreatedAndCallIt("partial void PreHandleInputAwake()", awakeMethod);
            awakeMethod.AddStatement(HierarchyHelper.MethodSignatureIntoCallStatement(HandleInputFiller.AwakeMethodSignature));
            c.EnsureMethodIsCreatedAndCallIt("partial void PostHandleInputAwake()", awakeMethod);
        }

        c.EnsureMethodIsCreatedAndCallIt(GeneratorHelper.Awake2MethodSignature, awakeMethod);
    }

    private static void CreateOnEnableHook(Class c, Hook hook) {
        var onEnableMethod = c.GetOrCreateMethod(GeneratorHelper.OnEnableMethodSignature);
        
        if (hook.CallBase) onEnableMethod.AddStatement("base.OnEnable();");

        if (hook.HookHandleInput) {
            c.EnsureMethodIsCreatedAndCallIt("partial void PreHandleInputOnEnable()", onEnableMethod);
            onEnableMethod.AddStatement(HierarchyHelper.MethodSignatureIntoCallStatement(HandleInputFiller.OnEnableMethodSignature));
            c.EnsureMethodIsCreatedAndCallIt("partial void PostHandleInputOnEnable()", onEnableMethod);
        }

        c.EnsureMethodIsCreatedAndCallIt(GeneratorHelper.OnEnable2MethodSignature, onEnableMethod);
    }
    
    private static void CreateOnDisableHook(Class c, Hook hook) {
        var onDisableMethod = c.GetOrCreateMethod(GeneratorHelper.OnDisableMethodSignature);
        
        if (hook.CallBase) onDisableMethod.AddStatement("base.OnDisable();");

        if (hook.HookHandleInput) {
            c.EnsureMethodIsCreatedAndCallIt("partial void PreHandleInputOnDisable()", onDisableMethod);
            onDisableMethod.AddStatement(HierarchyHelper.MethodSignatureIntoCallStatement(HandleInputFiller.OnDisableMethodSignature));
            c.EnsureMethodIsCreatedAndCallIt("partial void PostHandleInputOnDisable()", onDisableMethod);
        }

        c.EnsureMethodIsCreatedAndCallIt(GeneratorHelper.OnDisable2MethodSignature, onDisableMethod);
    }
}

public readonly record struct Hook(string FullyQualifiedClassName, 
    bool HookGetComponent = false, bool HookHandleInput = false, bool HookCollect = false,
    bool CallBase = false);