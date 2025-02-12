using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace UnityExtended.Generator;

public class DisplayAttributeData : IGenerateClass {
    private const string Usings = """
                                  UnityEngine
                                  UnityEngine.UIElements
                                  System
                                  System.Reflection
                                  UnityExtended.Core.Extensions
                                  """;
    
    private readonly string targetFieldName;
    private readonly Method createGUI, update;
    
    public Class GeneratedClass { get; }

    public DisplayAttributeData(INamedTypeSymbol classSymbol, IFieldSymbol[] fields, Compilation compilation) {
        string classFQName = classSymbol.ToDisplayString();
        var (namespaceName, className) = classFQName.SeparateFromFullyQualifiedName();
        targetFieldName = className.ToLowerFirst();
        
        GeneratedClass = new Class(classFQName + "Inspector");
        GeneratedClass.AddImplementation("UnityEditor.Editor");
        GeneratedClass.AddAttribute($"[UnityEditor.CustomEditor(typeof({classFQName})), UnityEditor.CanEditMultipleObjects]");
        GeneratedClass.AddField($"private {classFQName} {targetFieldName};");
        GeneratedClass.Constraints.Add(GeneratorHelper.UnityEditorConstraint);
        GeneratedClass.AddUsing(Usings);

        createGUI = new Method(GeneratorHelper.CreateInspectorGUISignature);
        update = new("private void Update()");
        Method modifyRoot = new("partial void ModifyRoot(ref VisualElement root)");
        Method update2 = new("partial void Update2()");
        
        GeneratedClass.AddMethods(createGUI, update, update2, modifyRoot);
        
        createGUI.AddStatement($"""
                               {targetFieldName} = ({classFQName})target;
                               var root = new VisualElement();
                               """);
        
        update.AddStatement("if (!Application.isPlaying) return;\n");

        foreach (var field in fields) {
            var fieldType = field.Type;

            if (fieldType.Name == "Single") {
                GenerateDataForAField(field, "root", classFQName);
            }
            else if (fieldType
                         .GetAttributes()
                         .FirstOrDefault(x => x.AttributeClass.ToDisplayString() == $"{GeneratorHelper.AttributesNamespace}.DisplayItemAttribute") 
                     is {} displayItemAttribute) {
                GenerateDataForADisplayItem(displayItemAttribute, field, compilation, classFQName);
            }
        }
        
        createGUI.AddStatement("""
                                // Add others
                                root.AddAllSerializedProperties(serializedObject);
                                
                                root.schedule.Execute(Update).Every(50);
                                
                                ModifyRoot(ref root);
                                
                                return root;
                                """);
        
        update.AddStatement("Update2();");
    }

    private void GenerateDataForADisplayItem(AttributeData displayItemAttribute, IFieldSymbol field, Compilation compilation, string originalClassName) {
        if (displayItemAttribute.ApplicationSyntaxReference == null) return;

        var displayFieldName = field.Name.NormalizeBackingFieldName();
        
        var attributeSyntax = (AttributeSyntax)displayItemAttribute.ApplicationSyntaxReference.GetSyntax();
        var argumentList = attributeSyntax.ArgumentList;
        
        if (argumentList == null) return;
        
        var arguments = argumentList.Arguments;

        var containerExpression = (TypeOfExpressionSyntax)arguments[0].Expression;
        var containerTypeSyntax = containerExpression.Type;
        var semanticModel = compilation.GetSemanticModel(containerTypeSyntax.SyntaxTree);
        var containerTypeSymbol = semanticModel.GetTypeInfo(containerTypeSyntax).Type;
        
        if (containerTypeSymbol == null) return;
        
        var containerVarName = $"{displayFieldName}{containerTypeSymbol.Name}";
                
        createGUI.AddStatement($$"""
                                 var {{containerVarName}} = new {{containerTypeSymbol.ToDisplayString()}} {
                                     text = "{{displayFieldName}}"
                                 };
                                 """);

        for (int i = 1; i < arguments.Count; i++) {
            var argument = arguments[i];
            var literalExpression = (LiteralExpressionSyntax)argument.Expression;
            var path = literalExpression.Token.ValueText;

            var membersAtPath = field.Type.GetMembers(path);

            if (membersAtPath.Any()) {
                var foundMember = membersAtPath.First();
                        
                if (foundMember is IFieldSymbol foundField) GenerateDataForAField(foundField, containerVarName, originalClassName);
                if (foundMember is IPropertySymbol propertySymbol) GenerateDataForAPropertyGetter(propertySymbol, containerVarName, $"{targetFieldName}.{displayFieldName}");
            }
        }
        
        createGUI.AddStatement($"""
                               
                               root.Add({containerVarName});
                               """);
    }

    private void GenerateDataForAField(IFieldSymbol field, string containerName, string originalClassName) {
        var fieldName = field.Name;
        var displayFieldName = fieldName.NormalizeBackingFieldName();

        string? controlName = TypeToControlName(field.Type);
        
        if (controlName == null) return;
        
        CreateControl(controlName, displayFieldName, containerName);

        bool isNotAccessible = field.DeclaredAccessibility != Accessibility.Public &&
                               field.DeclaredAccessibility != Accessibility.Internal;
        if (isNotAccessible) {
            string reflectionFieldName = $"{displayFieldName}Field";

            GeneratedClass.AddField($"private FieldInfo {reflectionFieldName};");
            createGUI.AddStatement(
                $"{reflectionFieldName} = typeof({originalClassName}).GetField(\"{fieldName}\", BindingFlags.NonPublic | BindingFlags.Instance);");


            update.AddStatement(
                $"{displayFieldName}.value = ({field.Type.Name}){reflectionFieldName}.GetValue({targetFieldName});");
        }
        else {
            update.AddStatement($"{displayFieldName}.value = {targetFieldName}.{displayFieldName};");
        }
    }

    private void GenerateDataForAPropertyGetter(IPropertySymbol propertySymbol, string containerName, string updateValuePath) {
        string classFQName = GeneratedClass.FullyQualifiedName;
        var propertyName = propertySymbol.Name;
        
        string? controlName = TypeToControlName(propertySymbol.Type);

        CreateControl(controlName, propertyName, containerName);
        
        update.AddStatement($"{propertyName}.value = {updateValuePath}.{propertyName};");
    }

    private void CreateControl(string controlName, string generatedFieldName, string containerName) {
        GeneratedClass.AddField($"private {controlName} {generatedFieldName};");

        // TODO: interpolation with raw literal
        createGUI.AddStatement($$"""
                                 {{generatedFieldName}} = new {{controlName}}() {
                                    label = "{{generatedFieldName}}",
                                    enabledSelf = false
                                 };

                                 {{containerName}}.Add({{generatedFieldName}});
                                 """);
    }

    private string? TypeToControlName(ITypeSymbol type) {
        var typeName = type.Name;

        if (typeName == "Single") return "FloatField";
        else return null;
    }

    public static IGenerate? Transform(GeneratorSyntaxContext context, CancellationToken _) {
        var semanticModel = context.SemanticModel;
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        var classSymbol = (INamedTypeSymbol)ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclaration);

        var fields = classSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Field).Select(x => (IFieldSymbol)x);
        List<IFieldSymbol> validFields = new();

        foreach (var field in fields) {
            if (field.GetAttributes().Any(x =>
                    x.AttributeClass.ToDisplayString() == $"{GeneratorHelper.AttributesNamespace}.DisplayAttribute")) {
                validFields.Add(field);
            }
        }

        if (validFields.Count == 0) return null;

        return new DisplayAttributeData(classSymbol, validFields.ToArray(), semanticModel.Compilation);
    }
}