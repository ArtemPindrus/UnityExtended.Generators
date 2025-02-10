using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityExtended.Generator;

public class FoldoutGroupAttributeData : IGenerateClass {
    public Class GeneratedClass { get; }

    public FoldoutGroupAttributeData(INamedTypeSymbol classSymbol, string groupName, params string[] fieldNames) {
        GeneratedClass = new Class(classSymbol.ToDisplayString());

        string fieldsParam = "";

        for (var i = 0; i < fieldNames.Length; i++) {
            var name = fieldNames[i];

            if (i == fieldNames.Length - 1) fieldsParam += $"nameof({name})";
            else fieldsParam += $"nameof({name}), ";
        }

        GeneratedClass.AddField($"""
                                 [UnityEngine.SerializeField]
                                 [EditorAttributes.FoldoutGroup("{groupName}", {fieldsParam})]
                                 private EditorAttributes.Void {groupName.ToLowerFirst()}Group;
                                 """);
    }
    
    public static IGenerate TransformIntoIGenerate(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var classSymbol = context.TargetSymbol.ContainingType;
        var startField = context.TargetSymbol;

        var classFields = classSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Field).ToImmutableArray();
        var startFieldIndex = classFields.IndexOf(startField, 0, SymbolEqualityComparer.Default);

        List<ISymbol> includedFields = new() {startField};

        for (var i = startFieldIndex + 1; i < classFields.Length; i++) {
            var member = classFields[i];
            
            includedFields.Add(member);

            if (member.GetAttributes().Any(x =>
                    x.AttributeClass.ToDisplayString() == "UnityExtended.Generators.Attributes.EndFoldoutGroupAttribute")) break;
        }

        var attributeSyntax = (AttributeSyntax)context.Attributes[0].ApplicationSyntaxReference.GetSyntax();
        var argument = (LiteralExpressionSyntax)attributeSyntax.ArgumentList.Arguments[0].Expression;
        var name = argument.Token.ValueText;

        return new FoldoutGroupAttributeData(classSymbol, name, includedFields.Select(x => x.Name).ToArray());
    }
}