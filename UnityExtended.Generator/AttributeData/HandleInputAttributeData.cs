using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityExtended.Generator;

public class HandleInputAttributeData : IGenerateClass {
    public const string PreAwakeMethodName = "PreAwakeHandleInput";
    
    public Class GeneratedClass { get; }

    private HandleInputAttributeData(ITypeSymbol classSymbol, ITypeSymbol[] actionMapClassSymbols) {
        GeneratedClass = new(classSymbol.ToDisplayString());
        
        Method awake = new(GeneratorHelper.AwakeMethodSignature);
        awake.AddStatement($"{PreAwakeMethodName}();");
        
        Method onEnable = new(GeneratorHelper.OnEnableMethodSignature);
        Method onDisable = new(GeneratorHelper.OnDisableMethodSignature);
        Method preAwake = new($"partial void {PreAwakeMethodName}()");
        
        GeneratedClass.AddMethods(awake, onEnable, onDisable, preAwake);

        foreach (var actionMapClassSymbol in actionMapClassSymbols) {
            string fqActionMapName = actionMapClassSymbol.ToDisplayString();
        
            (string inputAssetFullyQualifiedName, string actionMapName) = fqActionMapName.SeparateFromFullyQualifiedName();
            string inputAssetClassName = inputAssetFullyQualifiedName.ExtractConcreteClassName();
        
            GeneratedClass.AddField($"private {fqActionMapName} {actionMapName};");
            
            awake.AddStatement($"{actionMapName} = UnityExtended.Core.Types.InputSingletonsManager.GetInstance<{inputAssetFullyQualifiedName}>().{actionMapName.Replace("Actions", "")};");
            
            var inputActions = actionMapClassSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Property);
            var methodsInClass = classSymbol.GetMembers().Where(x => x is IMethodSymbol methodSymbol).ToArray();

            foreach (var inputActionSymbol in inputActions) {
                var actionName = inputActionSymbol.Name;

                foreach (var postfix in GeneratorHelper.InputActionPostfixes) {
                    string methodName = $"{inputAssetClassName}_{actionMapName}_On{actionName}{postfix}";
                    string methodSignature = $"partial void {methodName}(UnityEngine.InputSystem.InputAction.CallbackContext callbackContext)";
                    GeneratedClass.AddMethod(new(methodSignature));

                    if (methodsInClass.Any(x => x.Name == methodName)) {
                        string subscriptionStatement =
                            $"{actionMapName}.{actionName}.{postfix.ToLower()} += {methodName};";
                    
                        onEnable.AddStatement(subscriptionStatement);
                        onDisable.AddStatement(subscriptionStatement.Replace('+', '-'));
                    }
                }
            }
        }
    }

    public static IGenerate? TransformToIGenerate(GeneratorAttributeSyntaxContext context,
        CancellationToken _) {
        var semanticModel = context.SemanticModel;

        var classSymbol = context.TargetSymbol;
        
        if (classSymbol is not ITypeSymbol validClassSymbol) return null;

        var attributeData = context.Attributes[0];

        // Get AttributeSyntax
        var syntaxReference = attributeData.ApplicationSyntaxReference;
        
        if (syntaxReference is null) return null;
        
        var attributeSyntax = (AttributeSyntax)syntaxReference.GetSyntax();
             
        if (attributeSyntax.ArgumentList is null) return null;
        
        // Get TypeSymbol
        var arguments = attributeSyntax.ArgumentList.Arguments;
        var actionMapSymbols = new ITypeSymbol[arguments.Count];

        for (var i = 0; i < arguments.Count; i++) {
            var argument = arguments[i];
            var expression = (TypeOfExpressionSyntax)argument.Expression;

            TypeSyntax actionMapType = expression.Type;
            ITypeSymbol? actionMapTypeSymbol = semanticModel.GetTypeInfo(actionMapType).Type;

            if (actionMapTypeSymbol is not null) actionMapSymbols[i] = actionMapTypeSymbol;
        }

        return new HandleInputAttributeData(validClassSymbol, actionMapSymbols);
    }
}