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

public readonly record struct Variable {
    public readonly string FullyQualifiedClassName;
    public readonly string? ContainingNamespace;
    public readonly string ContainingClass;
    public readonly string TypeName;
    public readonly string VariableName;

    public Variable(string fullyQualifiedClassName, ISymbol typeSymbol, string variableName) {
        FullyQualifiedClassName = fullyQualifiedClassName;
        (ContainingNamespace, ContainingClass) = fullyQualifiedClassName.SeparateFromFullyQualifiedName();
        
        TypeName = typeSymbol.ToDisplayString();
        
        VariableName = variableName;
    }
}

public readonly record struct HandleInputData {
    /// <summary>
    /// Fully qualified name of decorated class.
    /// </summary>
    public readonly string FullyQualifiedClassName;
    
    /// <summary>
    /// Fully qualified names of action maps.
    /// </summary>
    public readonly string[] ActionMapTypes;

    public readonly string[] PartialMethodNames;

    public HandleInputData(string fullyQualifiedClassName, string[] actionMapTypes, string[] partialMethodNames) {
        FullyQualifiedClassName = fullyQualifiedClassName;
        ActionMapTypes = actionMapTypes;
        PartialMethodNames = partialMethodNames;
    }
}

public readonly record struct ActionMap {
    public readonly string MapName;
    public readonly string[] Actions;

    public ActionMap(string mapName, string[] actions) {
        Actions = actions;
        MapName = mapName;
    }
}

public readonly record struct InputAsset {
    public readonly string FullyQualifiedClassName;
    public readonly ActionMap[] ActionMaps;

    public InputAsset(string fullyQualifiedClassName, ActionMap[] actionMaps) {
        FullyQualifiedClassName = fullyQualifiedClassName;
        ActionMaps = actionMaps;
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
            static (_,_) => true, TransformIntoVariable)
            .Where(x => x is not null)
            .Select((v, _) => v!.Value)
            .Collect();
        
        var inputInfoProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: InputInfoPredicate, 
            transform: TransformToInputInfo)
            .Where(x => x is not null)
            .Select((info, _) => info!.Value)
            .Collect();
        
        var handleInputProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                "UnityExtended.Generator.Attributes.HandleInputAttribute",
                static (_,_) => true, TransformToHandleInputInfo)
            .Where(x => x is not null)
            .Select((v, _) => v!.Value)
            .Collect();

        var provider = variableProvider.Combine(inputInfoProvider).Combine(handleInputProvider);
        
        context.RegisterSourceOutput(provider, Execute);

    }

    private static HandleInputData? TransformToHandleInputInfo(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var semanticModel = context.SemanticModel;

        var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
        var classSymbol = context.TargetSymbol;
        var className = classSymbol.ToDisplayString();

        List<string> typeNames = new();
        foreach (var attributeData in context.Attributes) {
            var syntaxReference = attributeData.ApplicationSyntaxReference;

            if (syntaxReference is null) continue;

            var attributeSyntax = (AttributeSyntax)syntaxReference.GetSyntax();
            
            if (attributeSyntax.ArgumentList is null) continue;

            var attributeArgumentSyntax = attributeSyntax.ArgumentList.Arguments[0];

            var expression = (TypeOfExpressionSyntax)attributeArgumentSyntax.Expression;
            
            var type = expression.Type;
            var typeSymbol = semanticModel.GetSymbolInfo(type).Symbol;
            
            if (typeSymbol == null) continue;
            
            var typeName = typeSymbol.ToDisplayString();
            typeNames.Add(typeName);
        }

        List<string> partialMethods = new();
        foreach (var member in classDeclarationSyntax.Members) {
            if (member is MethodDeclarationSyntax methodDeclarationSyntax &&
                methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)) {
                var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
                
                if (methodSymbol is null) continue;
                
                partialMethods.Add(methodSymbol.Name);
            }
        }

        return new(className, typeNames.ToArray(), partialMethods.ToArray());
    }

    private static InputAsset? TransformToInputInfo(GeneratorSyntaxContext context, CancellationToken _) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        // check if implements interface IInputActionCollection2
        if (!classDeclarationSyntax.Implements(context.SemanticModel,
                "UnityEngine.InputSystem.IInputActionCollection2")) return null;
        
        // check for InputActions
        List<ActionMap> inputActionsInfos = new();

        foreach (var memberDeclarationSyntax in classDeclarationSyntax.Members) {
            if (memberDeclarationSyntax is not StructDeclarationSyntax structDeclarationSyntax) continue;

            var structSymbol = context.SemanticModel.GetDeclaredSymbol(structDeclarationSyntax);
            
            if (structSymbol is null) continue;
            
            var structName = structSymbol.Name;
            
            if (!structName.EndsWith("Actions")) continue;

            List<string> actionNames = new();

            foreach (var property in structDeclarationSyntax.Members.GetEveryPropertyOfType(context.SemanticModel, "UnityEngine.InputSystem.InputAction")) {
                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(property);
                
                if (propertySymbol is null) continue;
                
                var propertyName = propertySymbol.Name;
                
                actionNames.Add(propertyName);
            }

            ActionMap actionMap = new(structName, actionNames.ToArray());
            inputActionsInfos.Add(actionMap);
        }

        if (inputActionsInfos.Count > 0) {
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

            if (classSymbol is null) return null;
            
            var className = classSymbol.ToDisplayString();

            return new InputAsset(className, inputActionsInfos.ToArray());
        }

        return new();
    }

    private static bool InputInfoPredicate(SyntaxNode node, CancellationToken _) {
        return node is ClassDeclarationSyntax { BaseList.Types.Count: > 0 };
    }

    private static void Execute(SourceProductionContext context, ((ImmutableArray<Variable> variables, ImmutableArray<InputAsset> inputInfos) tuple, ImmutableArray<HandleInputData> handleInputDatas) providerTuple) {
        (ImmutableArray<Variable> variables, ImmutableArray<InputAsset> inputAssets) = providerTuple.tuple;
        ImmutableArray<HandleInputData> handleInputDatas = providerTuple.handleInputDatas;

        // find all necessary classes as fully qualified name
        HashSet<string> classesToGenerate = new();
        foreach (var variable in variables) {
            classesToGenerate.Add(variable.FullyQualifiedClassName);
        }

        foreach (var handleInput in handleInputDatas) {
            classesToGenerate.Add(handleInput.FullyQualifiedClassName);
        }

        foreach (var classToGenerate in classesToGenerate) {
            var validHandleInputDatas = handleInputDatas.Where(x => x.FullyQualifiedClassName == classToGenerate).ToArray();

            GetRequiredDataForHandleInput(handleInputDatas, inputAssets,
                out List<string> partialMethodsToBuild, out List<string> inputSubscriptions);
            
            IndentedStringBuilder stringBuilder = new();

            var (containingNamespace, containingClass) = classToGenerate.SeparateFromFullyQualifiedName();

            if (containingNamespace != null)
                stringBuilder.AppendLine($"namespace {containingNamespace} {{").IncrementIndent();

            stringBuilder.AppendLine($"public partial class {containingClass} {{").IncrementIndent();
            
            // create fields
            foreach (var handleInput in validHandleInputDatas) {
                foreach (var actionType in handleInput.ActionMapTypes) {
                    string concreteTypeName = actionType.ExtractConcreteClassName();
                    stringBuilder.AppendLine($"private {actionType} {concreteTypeName};");
                }
            }

            stringBuilder.AppendLine();
            
            // create Awake
            stringBuilder.AppendLine("private void Awake() {").IncrementIndent();
            
            //// get components
            foreach (var variable in variables.Where(x => x.FullyQualifiedClassName == classToGenerate)) {
                stringBuilder.AppendLine($"{variable.VariableName} = GetComponent<{variable.TypeName}>();");
            }

            stringBuilder.AppendLine();
            
            //// field init
            foreach (var handleInput in validHandleInputDatas) {
                foreach (var fullyQualifiedActionMapTypeName in handleInput.ActionMapTypes) {
                    (string inputAssetName, string actionMapName) =
                        fullyQualifiedActionMapTypeName.SeparateFromFullyQualifiedName();
                    
                    stringBuilder.AppendLine($"{actionMapName} = UnityExtended.Core.Types.InputSingletonsManager.GetInstance<{inputAssetName}>().{actionMapName.Replace("Actions", "")};");
                }
            }

            stringBuilder.AppendLine("Awake2();");
            stringBuilder.DecrementIndent().AppendLine("}").AppendLine(); 
            // close Awake

            stringBuilder.AppendLine("partial void Awake2();").AppendLine();

            // OnEnable
            stringBuilder.AppendLine("private void OnEnable() {").IncrementIndent();
            
            foreach (var inputSubscription in inputSubscriptions) {
                stringBuilder.AppendLine(inputSubscription);
            }

            stringBuilder.DecrementIndent().AppendLine("}").AppendLine();
            // close OnEnable
            
            // OnDisable
            stringBuilder.AppendLine("private void OnDisable() {").IncrementIndent();
            
            foreach (var inputSubscription in inputSubscriptions) {
                stringBuilder.AppendLine(inputSubscription.Replace('+', '-'));
            }

            stringBuilder.DecrementIndent().AppendLine("}").AppendLine();
            // close OnDisable
            
            // TODO: build methods for inputs to match
            foreach (var method in partialMethodsToBuild) {
                stringBuilder.AppendLine(method);
            }
            
            stringBuilder.DecrementIndent().AppendLine("}"); // close Class
            
            if (containingNamespace != null) stringBuilder.DecrementIndent().AppendLine("}"); // close namespace
            
            context.AddSource($"{containingClass}.g.cs", stringBuilder.ToString());
        }
    }

    private static void GetRequiredDataForHandleInput(ImmutableArray<HandleInputData> attributesData, ImmutableArray<InputAsset> inputAssets,
        out List<string> partialMethodsToBuild, out List<string> inputSubscriptions) {
        partialMethodsToBuild = new();
        inputSubscriptions = new();

        foreach (var handleInput in attributesData) {
            foreach (string inputMapType in handleInput.ActionMapTypes) {
                (string inputMapAssetName, string actionMapName) = inputMapType.SeparateFromFullyQualifiedName();
                InputAsset? inputAsset =
                    inputAssets.FirstOrDefault(x => x.FullyQualifiedClassName == inputMapAssetName);

                if (inputAsset is not { } validAsset) continue;

                ActionMap? actionMap = validAsset.ActionMaps.FirstOrDefault(x => x.MapName == actionMapName);

                if (actionMap is not { } validMap) continue;

                foreach (var actionName in validMap.Actions) {
                    string performedSignature =
                        $"partial void On{validMap.MapName}_{actionName}Performed(UnityEngine.InputSystem.InputAction.CallbackContext context);";

                    string canceledSignature =
                        $"partial void On{validMap.MapName}_{actionName}Canceled(UnityEngine.InputSystem.InputAction.CallbackContext context);";

                    string startedSignature =
                        $"partial void On{validMap.MapName}_{actionName}Started(UnityEngine.InputSystem.InputAction.CallbackContext context);";

                    partialMethodsToBuild.Add(performedSignature);
                    partialMethodsToBuild.Add(canceledSignature);
                    partialMethodsToBuild.Add(startedSignature);

                    string performedName = $"On{validMap.MapName}_{actionName}Performed";
                    string canceledName = $"On{validMap.MapName}_{actionName}Canceled";
                    string startedName = $"On{validMap.MapName}_{actionName}Started";

                    if (handleInput.PartialMethodNames.Contains(performedName))
                        inputSubscriptions.Add($"{validMap.MapName}.{actionName}.performed += {performedName};");
                    if (handleInput.PartialMethodNames.Contains(canceledName))
                        inputSubscriptions.Add($"{validMap.MapName}.{actionName}.canceled += {canceledName};");
                    if (handleInput.PartialMethodNames.Contains(startedName))
                        inputSubscriptions.Add($"{validMap.MapName}.{actionName}.started += {startedName};");
                }
            }
        }
    }

    private static Variable? TransformIntoVariable(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var variableDeclaratorSyntax = (VariableDeclaratorSyntax)context.TargetNode;
        var variableDeclarationSyntax = (VariableDeclarationSyntax)variableDeclaratorSyntax.Parent;
        var typeSyntax = variableDeclarationSyntax.Type;
        
        var variableDeclaratorSymbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, variableDeclaratorSyntax);
        var classSymbol = variableDeclaratorSymbol.ContainingSymbol;
        var typeSymbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, typeSyntax).Symbol;

        if (typeSymbol is null) return null;

        var className = classSymbol.ToDisplayString();
        var variableName = variableDeclaratorSymbol.Name;

        return new Variable(className, typeSymbol, variableName);
    }
}