using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityExtended.Generator;

public class GetComponentAttributeData : IGenerateClass {
    public record struct GetComponentInstance(ITypeSymbol Type, string VariableName);

    public const string PreSignatureName = "PreGetComponent";
    private static List<ITypeSymbol> generatedClasses = new();
    
    public Class GeneratedClass { get; }

    private GetComponentAttributeData(ITypeSymbol classSymbol, GetComponentInstance[] getComponentInstances) {
        GeneratedClass = new Class(classSymbol.ToDisplayString());
        
        generatedClasses.Add(classSymbol);
        Method awake = new Method(GeneratorHelper.AwakeMethodSignature);
        awake.AddStatement($"{PreSignatureName}();");
        
        Method pre = new Method($"partial void {PreSignatureName}()");

        foreach (var instance in getComponentInstances) {
            string typeName = instance.Type.ToDisplayString();
            awake.AddStatement($"{instance.VariableName} = GetComponent<{typeName}>();");
        }
        
        GeneratedClass.AddMethods(awake, pre);
    }

    public static GetComponentAttributeData? Transform(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var classSymbol = context.TargetSymbol.ContainingType;
        
        var attributeClass = context.Attributes[0].AttributeClass;

        if (generatedClasses.Any(x => SymbolEqualityComparer.Default.Equals(classSymbol, x))) return null;
        
        List<GetComponentInstance> instances = new();

        var fields = classSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Field).Select(x => (IFieldSymbol)x);

        foreach (var field in fields) {
            if (field.GetAttributes()
                    .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeClass)) is
                { } attributeData) {
                instances.Add(new(field.Type, field.Name));
            }
        }

        return new GetComponentAttributeData(classSymbol, instances.ToArray());
    }
    
    public static IGenerate? TransformIntoIGenerate(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        return Transform(context, _);
    }

    public static void ClearCachedData() {
        generatedClasses.Clear();
    }
}