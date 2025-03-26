using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityExtended.Generator.Extensions;
using UnityExtended.Generator.Helpers;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace UnityExtended.Generator;

public class DisplayAttributeData : IGenerateClass {
    private readonly string baseClassName;
    
    public CustomEditorClass CustomEditorClass { get; }
    public Class GeneratedClass => CustomEditorClass;

    public DisplayAttributeData(INamedTypeSymbol classSymbol, IFieldSymbol displayFieldSymbol) {
        baseClassName = classSymbol.ToDisplayString();
        CustomEditorClass = CustomEditorClass.GetFor(baseClassName);

        var visualElementForField = CreateVisualElementForField(displayFieldSymbol);
        CustomEditorClass.AddVisualElementToRoot(visualElementForField);
    }

    /// <summary>
    /// Creates VisualElement that should display value of <paramref name="displayFieldSymbol"/>.
    /// </summary>
    /// <param name="displayFieldSymbol">Field that should be displayed.</param>
    /// <returns></returns>
    private VisualElementField CreateVisualElementForField(IFieldSymbol displayFieldSymbol, string? inspectorFieldPrefix = null) {
        var displayFieldType = displayFieldSymbol.Type;
        var displayFieldName = displayFieldSymbol.Name;
        var inspectorVisualElementFieldName = inspectorFieldPrefix + displayFieldName.NormalizeBackingFieldName();
            
        int order = FindOrderOfDisplayField(displayFieldSymbol);
        
        if (displayFieldType.GetAttribute($"{AttributesHelper.AttributesNamespace}.DisplayItemAttribute",
                out var displayItemAttribute)) {
            displayItemAttribute.GetParamValueAt(0, out INamedTypeSymbol? containerType);

            VisualElementField container = new(inspectorVisualElementFieldName, displayFieldType.ToDisplayString(), order, $"""
                 label = "{inspectorVisualElementFieldName}",
                 enabledSelf = false
                 """);

            var fieldsWithDisplay = displayFieldType.GetMembers()
                .Where(x => x.Kind == SymbolKind.Field && x.GetAttribute($"{AttributesHelper.AttributesNamespace}.DisplayAttribute", out var _));

            foreach (IFieldSymbol f in fieldsWithDisplay) {
                var child = CreateVisualElementForField(f, $"{container.FieldName}_");
                container.AddChild(child);
            }
            
            return container;
        }
        else {
            VisualElementField ve = new(inspectorVisualElementFieldName, displayFieldType.ToDisplayString(), order, $"""
                 label = "{inspectorVisualElementFieldName}",
                 enabledSelf = false
                 """);
            
            GenerateInspectorFieldUpdaters(displayFieldSymbol);

            return ve;
        }
    }

    private int FindOrderOfDisplayField(IFieldSymbol displayFieldSymbol) {
        int order = 0;
        
        if (displayFieldSymbol.GetAttribute($"{AttributesHelper.AttributesNamespace}.SetVisualElementAt",
                out var setVisualElementAtAttribute)) {
            if (setVisualElementAtAttribute.GetParamValueAt(0, out int foundOrder)) order = foundOrder;
        }

        return order;
    }

    private void GenerateInspectorFieldUpdaters(IFieldSymbol displayFieldSymbol) {
        var displayFieldName = displayFieldSymbol.Name;
        
        bool isNotAccessible = displayFieldSymbol.DeclaredAccessibility != Accessibility.Public &&
                               displayFieldSymbol.DeclaredAccessibility != Accessibility.Internal;
        
        if (isNotAccessible) {
            string reflectionFieldName = $"{displayFieldName}Field";

            GeneratedClass.AddFields($"private FieldInfo {reflectionFieldName};");
            CustomEditorClass
                .AddCreateGUIStatements($"{reflectionFieldName} = typeof({baseClassName}).GetField(\"{displayFieldName}\", BindingFlags.NonPublic | BindingFlags.Instance);")
                .AddUpdateStatements($"{displayFieldName}.value = ({displayFieldSymbol.Type.ToDisplayString()}){reflectionFieldName}.GetValue(targetCasted);");
        }
        else {
            CustomEditorClass.AddUpdateStatements($"{displayFieldName}.value = targetCasted.{displayFieldName};");
        }
    }
    
    
    

    private void GenerateDataForADisplayItem(AttributeData displayItemAttribute, IFieldSymbol field, Compilation compilation, string originalClassName) {
        // TODO: return later
        return;
        
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
                
        CustomEditorClass.AddCreateGUIStatements($$"""
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
                if (foundMember is IPropertySymbol propertySymbol) GenerateDataForAPropertyGetter(propertySymbol, containerVarName, $"targetCasted.{displayFieldName}");
            }
        }
    }

    private void GenerateDataForAField(IFieldSymbol field, string containerName, string originalClassName) {
        var fieldName = field.Name;
        var displayFieldName = fieldName.NormalizeBackingFieldName();

        // find order
        var setVisualElementAtAttribute = field.GetAttributes().FirstOrDefault(x =>
            x.AttributeClass is { Name: $"{AttributesHelper.AttributesNamespace}.SetVisualElementAt" });

        int order = -1;
        if (setVisualElementAtAttribute != null) {
            if (setVisualElementAtAttribute.GetParamValueAt(0, out int foundOrder)) order = foundOrder;
        }
        
        //
        CreateControl(field.Type, displayFieldName, containerName, order);

        bool isNotAccessible = field.DeclaredAccessibility != Accessibility.Public &&
                               field.DeclaredAccessibility != Accessibility.Internal;
        if (isNotAccessible) {
            string reflectionFieldName = $"{displayFieldName}Field";

            GeneratedClass.AddFields($"private FieldInfo {reflectionFieldName};");
            CustomEditorClass
                .AddCreateGUIStatements($"{reflectionFieldName} = typeof({originalClassName}).GetField(\"{fieldName}\", BindingFlags.NonPublic | BindingFlags.Instance);")
                .AddUpdateStatements($"{displayFieldName}.value = ({field.Type.Name}){reflectionFieldName}.GetValue(targetCasted);");
        }
        else {
            CustomEditorClass.AddUpdateStatements($"{displayFieldName}.value = targetCasted.{displayFieldName};");
        }
    }

    private void GenerateDataForAPropertyGetter(IPropertySymbol propertySymbol, string containerName, string updateValuePath) {
        string classFQName = GeneratedClass.FullyQualifiedName;
        var propertyName = propertySymbol.Name;
        
        CreateControl(propertySymbol.Type, propertyName, containerName);
        
        CustomEditorClass.AddUpdateStatements($"{propertyName}.value = {updateValuePath}.{propertyName};");
    }

    private void CreateControl(ITypeSymbol fieldType, string fieldName, string containerName, int order = -1) {
        string controlTypeName = FieldTypeToControlType(fieldType);
        
        VisualElementField visualElementField = new(fieldName, controlTypeName, order, $"""
                                                                          label = "{fieldName}",
                                                                          enabledSelf = false
                                                                          """);
        
        //CustomEditorClass.AddVisualElementToRoot(visualElement, containerName);
    }

    private string? FieldTypeToControlType(ITypeSymbol type) {
        var typeName = type.Name;

        if (typeName == "Single") return "FloatField";
        else return null;
    }

    public static IGenerate? Transform(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var classSymbol = context.TargetSymbol.ContainingType;
        var fieldSymbol = (IFieldSymbol)context.TargetSymbol;

        if (classSymbol.GetAttribute($"{AttributesHelper.AttributesNamespace}.DisplayItemAttribute", out var _)) return null;

        return new DisplayAttributeData(classSymbol, fieldSymbol);
    }
}