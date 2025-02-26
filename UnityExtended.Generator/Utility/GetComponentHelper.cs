using System;
using Hierarchy;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator.Utility;

public static class GetComponentHelper {
    public static string CreateStatement(GetComponentInstance instance) {
        string typeName = instance.Type.ToDisplayString().Replace("[]", "");
        string postfix = instance.Plural ? "s" : "";
        postfix += instance.InParam switch {
            In.Self => "",
            In.Children => "InChildren",
            In.Parent => "InParent",
            _ => throw new ArgumentException()
        };

        return $"{instance.VariableName} = GetComponent{postfix}<{typeName}>();";
    }

    public static Class CreateBareClass(ITypeSymbol classSymbol, GetComponentInstance getComponentInstance, string mainMethodSignature, string preMethodSignature) {
        var generatedClass = new Class(classSymbol.ToDisplayString());
        
        Method main = new Method(mainMethodSignature);
        Method pre = new Method(preMethodSignature);

        main.AddStatement(CreateStatement(getComponentInstance));
        
        generatedClass.AddMethods(main, pre);

        return generatedClass;
    }

    public static GetComponentInstance CreateInstance(GeneratorAttributeSyntaxContext context) {
        var targetSymbol = (IFieldSymbol)context.TargetSymbol;

        var attributeData = context.Attributes[0];
        
        attributeData.GetParamValueAt(0, out In inParam);
        attributeData.GetParamValueAt(1, out bool plural);

        GetComponentInstance instance = new(targetSymbol.Type, targetSymbol.Name, inParam, plural);
        
        return instance;
    }
}