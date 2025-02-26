using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityExtended.Generator.Utility;

namespace UnityExtended.Generator;

public enum In {
    Self,
    Children,
    Parent
}

public partial class GetComponentAttributeData : IGenerateClass {
    public const string PreSignatureName = "PreGetComponent";
    
    public Class GeneratedClass { get; }

    public static ImmutableArray<IGenerate> Assemble(IEnumerable<GetComponentAttributeData> instances) {
        var assembly = ImmutableArray.CreateBuilder<IGenerate>();
        var groupedByClass = instances.GroupBy(x => x.GeneratedClass.FullyQualifiedName);
        
        foreach (var group in groupedByClass) {
            var first = group.First();
            
            foreach (var data in group.Skip(1)) {
                first.GeneratedClass.Merge(data.GeneratedClass);
            }

            var awake = first.GeneratedClass.Methods.First(x => x.Signature == GeneratorHelper.AwakeMethodSignature);
            awake.InsertStatement($"{PreSignatureName}();", 0);
            
            assembly.Add(first);
        }

        return assembly.ToImmutable();
    }

    private GetComponentAttributeData(ITypeSymbol classSymbol, GetComponentInstance getComponentInstance) {
        GeneratedClass = GetComponentHelper.CreateBareClass(classSymbol, getComponentInstance, 
            GeneratorHelper.AwakeMethodSignature, $"partial void {PreSignatureName}()");
    }

    public static GetComponentAttributeData? Transform(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var classSymbol = context.TargetSymbol.ContainingType;

        GetComponentInstance instance = GetComponentHelper.CreateInstance(context);

        return new GetComponentAttributeData(classSymbol, instance);
    }
    
    public static IGenerate? TransformIntoIGenerate(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        return Transform(context, _);
    }
}