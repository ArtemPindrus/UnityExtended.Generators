using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using UnityExtended.Generator.Utility;

namespace UnityExtended.Generator;

public class GetComponentAheadAttributeData : IGenerateClass {
    public const string PreSignatureName = "PreGetComponentAhead";
    
    public Class GeneratedClass { get; }
    
    public static ImmutableArray<IGenerate> Assemble(IEnumerable<GetComponentAheadAttributeData> instances) {
        var assembly = ImmutableArray.CreateBuilder<IGenerate>();
        var groupedByClass = instances.GroupBy(x => x.GeneratedClass.FullyQualifiedName);
        
        foreach (var group in groupedByClass) {
            var first = group.First();
            
            foreach (var data in group.Skip(1)) {
                first.GeneratedClass.Merge(data.GeneratedClass);
            }

            var onValidate = first.GeneratedClass.Methods.First(x => x.Signature == GeneratorHelper.OnValidateMethodSignature);
            onValidate.InsertStatement($"{PreSignatureName}();", 0);
            onValidate.InsertStatement("UnityEditor.EditorApplication.delayCall += ()=> {",1);
            onValidate.AddStatement("};");
            
            assembly.Add(first);
        }

        return assembly.ToImmutable();
    }

    private GetComponentAheadAttributeData(ITypeSymbol classSymbol, GetComponentInstance getComponentInstance) {
        GeneratedClass = GetComponentHelper.CreateBareClass(classSymbol, getComponentInstance, 
            GeneratorHelper.OnValidateMethodSignature, $"partial void {PreSignatureName}()");
    }
    
    public static GetComponentAheadAttributeData? Transform(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var classSymbol = context.TargetSymbol.ContainingType;

        GetComponentInstance instance = GetComponentHelper.CreateInstance(context);

        return new GetComponentAheadAttributeData(classSymbol, instance);
    }
}