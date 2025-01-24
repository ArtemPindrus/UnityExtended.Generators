using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnityExtended.Generator;

public readonly record struct GetComponentAttributeData : IGenerateClass, IGenerateMethod, IGenerateStatements {
    public Class GeneratedClass { get; }

    public Method Method { get; }
    
    public List<StatementDeclaration> Statements { get; }

    private GetComponentAttributeData(string fullyQualifiedClassName, ITypeSymbol typeSymbol, string variableName) {
        (string? namespaceName, string className) = fullyQualifiedClassName.SeparateFromFullyQualifiedName();

        GeneratedClass = new Class(fullyQualifiedClassName, namespaceName, className);

        string typeName = typeSymbol.ToDisplayString();
        Method = new Method(GeneratorHelper.AwakeMethodSignature, fullyQualifiedClassName);
        Statements = [
            new StatementDeclaration($"{variableName} = GetComponent<{typeName}>();", Method)
        ];
    }
    
    public static IGenerate? TransformIntoIGenerate(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var variableDeclaratorSyntax = (VariableDeclaratorSyntax)context.TargetNode;
        var variableDeclarationSyntax = (VariableDeclarationSyntax)variableDeclaratorSyntax.Parent;
        var typeSyntax = variableDeclarationSyntax.Type;
        
        var variableDeclaratorSymbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, variableDeclaratorSyntax);
        var classSymbol = variableDeclaratorSymbol.ContainingSymbol;
        var typeSymbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, typeSyntax).Symbol;

        if (typeSymbol is not ITypeSymbol validTypeSymbol) return null;

        var fullyQualifiedClassName = classSymbol.ToDisplayString();
        var variableName = variableDeclaratorSymbol.Name;

        return new GetComponentAttributeData(fullyQualifiedClassName, validTypeSymbol, variableName);
    }
}

public readonly record struct HandleInputAttributeData : IGenerateClass, IGenerateMethods, IGenerateField, IGenerateStatements {
    public Class GeneratedClass { get; }
    public List<Method> Methods { get; }
    public List<StatementDeclaration> Statements { get; } // TODO: combine into Method
    public string FieldDeclaration { get; }

    private HandleInputAttributeData(ITypeSymbol classSymbol, ITypeSymbol actionMapSymbol, string[] inputActionNames, IMethodSymbol[] partialClassMethods) {
        string fullyQualifiedClassName = classSymbol.ToDisplayString();

        (string? namespaceName, string className) = fullyQualifiedClassName.SeparateFromFullyQualifiedName();

        GeneratedClass = new Class(fullyQualifiedClassName, namespaceName, className);
        
        string fqActionMapTypeName = actionMapSymbol.ToDisplayString();
        
        (string inputAssetFullyQualifiedName, string actionMapName) = fqActionMapTypeName.SeparateFromFullyQualifiedName();
        string inputAssetClassName = inputAssetFullyQualifiedName.ExtractConcreteClassName();

        FieldDeclaration = $"private {fqActionMapTypeName} {actionMapName};";

        Methods = [
            new Method(GeneratorHelper.AwakeMethodSignature, fullyQualifiedClassName),
            new Method(GeneratorHelper.OnEnableMethodSignature, fullyQualifiedClassName),
            new Method(GeneratorHelper.OnDisableMethodSignature, fullyQualifiedClassName),
        ];
        
        Statements = [
            new ($"{actionMapName} = UnityExtended.Core.Types.InputSingletonsManager.GetInstance<{inputAssetFullyQualifiedName}>().{actionMapName.Replace("Actions", "")};", Methods[0]),
        ];

        foreach (var inputActionName in inputActionNames) {
            foreach (var postfix in GeneratorHelper.InputActionPostfixes) {
                string methodName = $"{inputAssetClassName}_On{inputActionName}{postfix}";
                string methodSignature = $"partial void {methodName}(UnityEngine.InputSystem.InputAction.CallbackContext callbackContext)";
                Methods.Add(new(methodSignature, fullyQualifiedClassName));

                if (partialClassMethods.Any(x => x.Name == methodName)) {
                    string subscriptionStatement =
                        $"{actionMapName}.{inputActionName}.{postfix.ToLower()} += {methodName};";
                    Statements.Add(new StatementDeclaration(subscriptionStatement, Methods[1]));
                    Statements.Add(new StatementDeclaration(subscriptionStatement.Replace('+', '-'), Methods[2]));
                }
            }
        }
    }

    public static IEnumerable<IGenerate> TransformToIGenerate(GeneratorAttributeSyntaxContext context,
        CancellationToken _) {
        var semanticModel = context.SemanticModel;

        var classNode = (ClassDeclarationSyntax)context.TargetNode;
        var classSymbol = context.TargetSymbol;
        
        if (classSymbol is not ITypeSymbol validClassSymbol) yield break;

        foreach (var attributeData in context.Attributes) {
            // Get AttributeSyntax
            var syntaxReference = attributeData.ApplicationSyntaxReference;
        
            if (syntaxReference is null) continue;
        
            var attributeSyntax = (AttributeSyntax)syntaxReference.GetSyntax();
             
            if (attributeSyntax.ArgumentList is null) continue;
        
            // Get TypeSymbol
            var arguments = attributeSyntax.ArgumentList.Arguments;
            var attributeArgumentSyntax = arguments[0];
        
            var expression = (TypeOfExpressionSyntax)attributeArgumentSyntax.Expression;
             
            var type = expression.Type;
            var typeSymbol = semanticModel.GetSymbolInfo(type).Symbol;
             
            if (typeSymbol is not ITypeSymbol validTypeSymbol) continue;

            // Get action names
            string[] actionNames = new string[arguments.Count - 1];

            for (int i = 1; i < arguments.Count; i++) {
                var argumentSyntax = arguments[i];

                string literal = "";

                if (argumentSyntax.Expression is LiteralExpressionSyntax stringExpression) {
                    literal = stringExpression.Token.ValueText;
                }
                else if (argumentSyntax.Expression is InvocationExpressionSyntax invocationExpression) {
                    var invocationArgumentExpressionSyntax = (MemberAccessExpressionSyntax)invocationExpression.ArgumentList.Arguments[0].Expression;
                    literal = invocationArgumentExpressionSyntax.Name.Identifier.ValueText;
                }

                actionNames[i - 1] = literal;
            }
            
            // get implemented partial methods
            IEnumerable<IMethodSymbol> partialMethods = classNode.Members
                .Where(x => x.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword)))
                .Select(x => (IMethodSymbol)semanticModel.GetDeclaredSymbol(x));

            yield return new HandleInputAttributeData(validClassSymbol, validTypeSymbol, actionNames, partialMethods.ToArray());
        }
    }
}

[Generator]
public class UnityGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource($"{nameof(GeneratorHelper.Attributes)}.g.cs",
                SourceText.From(GeneratorHelper.Attributes, Encoding.UTF8));
        });

        var variableProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "UnityExtended.Generator.Attributes.GetComponentAttribute",
            static (_,_) => true, GetComponentAttributeData.TransformIntoIGenerate)
            .Where(x => x is not null)
            .Collect();
        
        var handleInputProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "UnityExtended.Generator.Attributes.HandleInputAttribute",
            static (_,_) => true, HandleInputAttributeData.TransformToIGenerate)
            .Collect();

        var provider = variableProvider.Combine(handleInputProvider).Select(Selector);
        
        context.RegisterSourceOutput(provider, Execute);

    }

    private static IEnumerable<IGenerate> Selector((ImmutableArray<IGenerate?>, ImmutableArray<IEnumerable<IGenerate>>) tuple, CancellationToken _) {
        (ImmutableArray<IGenerate?> left, ImmutableArray<IEnumerable<IGenerate>> right) = tuple;

        foreach (var iGenerate in left) {
            if (iGenerate is null) continue;
            yield return iGenerate;
        }

        foreach (var enumerable in right) {
            foreach (var iGenerate in enumerable) {
                yield return iGenerate;
            }
        }
    }

    private static void Execute(SourceProductionContext context, IEnumerable<IGenerate> requiredGeneratedData) {
        ExtractGeneratedClassesFromData(requiredGeneratedData, out Dictionary<string, Class> classesToGenerate); // hashed by FQName

        AddSecondMethodsIfNecessary(classesToGenerate.Values);
        
        foreach (var classToGenerate in classesToGenerate.Values) {
            IndentedStringBuilder stringBuilder = new();

            // open Namespace
            if (classToGenerate.NamespaceName is { } namespaceName)
                stringBuilder.AppendLine($"namespace {namespaceName} {{").IncrementIndent();

            // open Class
            stringBuilder.AppendLine($"partial class {classToGenerate.ClassName} {{").IncrementIndent();

            //// fields
            foreach (var fieldDeclaration in classToGenerate.Fields) {
                stringBuilder.AppendLine(fieldDeclaration);
            }

            stringBuilder.AppendLine();

            //// methods
            foreach (var method in classToGenerate.Methods) {
                if (method.MethodSignature.Contains("partial")) {
                    stringBuilder.AppendLine($"{method.MethodSignature};").AppendLine();
                }
                else {
                    stringBuilder.AppendLine($"{method.MethodSignature} {{").IncrementIndent();
                
                    foreach (var statement in method.Statements) {
                        stringBuilder.AppendLine(statement);
                    }

                    stringBuilder.DecrementIndent().AppendLine("}").AppendLine();
                }
            }

            stringBuilder.DecrementIndent().AppendLine("}"); 
            // close Class

            if (classToGenerate.NamespaceName is not null)
                stringBuilder.DecrementIndent().AppendLine("}");
            // close Namespace
            
            context.AddSource($"{classToGenerate.FullyQualifiedClassName}.g.cs", stringBuilder.ToString());
        }
    }

    private static void ExtractGeneratedClassesFromData(IEnumerable<IGenerate> requiredGeneratedData,
        out Dictionary<string, Class> classesToGenerate) {
        classesToGenerate = new();
        
        foreach (var generateData in requiredGeneratedData) {
            if (generateData is IGenerateClass classGenerator) {
                if (!classesToGenerate.ContainsKey(classGenerator.GeneratedClass.FullyQualifiedClassName)) {
                    classesToGenerate.Add(classGenerator.GeneratedClass.FullyQualifiedClassName,
                        classGenerator.GeneratedClass);
                }
            }
            if (generateData is IGenerateMethods methodsGenerator) {
                if (classesToGenerate.TryGetValue(methodsGenerator.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    existingClass.TryAddMethods(methodsGenerator.Methods);
                }
                else {
                    classesToGenerate.Add(methodsGenerator.GeneratedClass.FullyQualifiedClassName, methodsGenerator.GeneratedClass);
                    methodsGenerator.GeneratedClass.TryAddMethods(methodsGenerator.Methods);
                }
            }
            if (generateData is IGenerateMethod methodGenerator) {
                if (classesToGenerate.TryGetValue(methodGenerator.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    existingClass.TryAddMethod(methodGenerator.Method);
                }
                else {
                    classesToGenerate.Add(methodGenerator.GeneratedClass.FullyQualifiedClassName, methodGenerator.GeneratedClass);
                    methodGenerator.GeneratedClass.TryAddMethod(methodGenerator.Method);
                }
            }
            if (generateData is IGenerateStatements generateStatements) {
                if (!classesToGenerate.TryGetValue(generateStatements.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    classesToGenerate.Add(generateStatements.GeneratedClass.FullyQualifiedClassName, generateStatements.GeneratedClass);
                    existingClass = generateStatements.GeneratedClass;
                }
                
                foreach (var statement in generateStatements.Statements) {
                    existingClass.GetOrAddMethod(statement.TargetMethod).TryAddStatement(statement);
                }
            }
            if (generateData is IGenerateStatement generateStatement) {
                if (!classesToGenerate.TryGetValue(generateStatement.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    classesToGenerate.Add(generateStatement.GeneratedClass.FullyQualifiedClassName, generateStatement.GeneratedClass);
                    existingClass = generateStatement.GeneratedClass;
                }
                
                existingClass.GetOrAddMethod(generateStatement.Method).TryAddStatement(generateStatement.Statement);
            }

            if (generateData is IGenerateField generateField) {
                if (!classesToGenerate.TryGetValue(generateField.GeneratedClass.FullyQualifiedClassName, out Class existingClass)) {
                    classesToGenerate.Add(generateField.GeneratedClass.FullyQualifiedClassName, generateField.GeneratedClass);
                    existingClass = generateField.GeneratedClass;
                }

                existingClass.TryAddField(generateField.FieldDeclaration);
            }
        }
    }

    private static void AddSecondMethodsIfNecessary(IEnumerable<Class> classesToGenerate) {
        foreach (var classToGenerate in classesToGenerate) {
            Method awakeMethod =
                classToGenerate.Methods.FirstOrDefault(x => x.MethodSignature == GeneratorHelper.AwakeMethodSignature);

            if (awakeMethod != default) {
                var awake2Method = new Method(GeneratorHelper.Awake2MethodSignature,
                    classToGenerate.FullyQualifiedClassName);
                classToGenerate.TryAddMethod(awake2Method);

                awakeMethod.TryAddStatement(new StatementDeclaration("Awake2();", awakeMethod));
            }
            
            Method onEnableMethod =
                classToGenerate.Methods.FirstOrDefault(x => x.MethodSignature == GeneratorHelper.OnEnableMethodSignature);
            
            if (onEnableMethod != default) {
                var onEnable2Method = new Method(GeneratorHelper.OnEnable2MethodSignature,
                    classToGenerate.FullyQualifiedClassName);
                classToGenerate.TryAddMethod(onEnable2Method);

                onEnableMethod.TryAddStatement(new StatementDeclaration("OnEnable2();", onEnableMethod));
            }
            
            Method onDisableMethod =
                classToGenerate.Methods.FirstOrDefault(x => x.MethodSignature == GeneratorHelper.OnDisableMethodSignature);
            
            if (onDisableMethod != default) {
                var onDisable2Method = new Method(GeneratorHelper.OnDisable2MethodSignature,
                    classToGenerate.FullyQualifiedClassName);
                classToGenerate.TryAddMethod(onDisable2Method);

                onDisableMethod.TryAddStatement(new StatementDeclaration("OnEnable2();", onDisableMethod));
            }
        }
    }
}