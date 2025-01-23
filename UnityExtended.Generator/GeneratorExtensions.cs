using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityExtended.Generator;

internal static class GeneratorExtensions {
    public static bool Implements(this ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel, string fullyQualifiedName) {
        if (classDeclarationSyntax.BaseList == null || classDeclarationSyntax.BaseList.Types.Count == 0) return false;
        
        foreach (var baseTypeSyntax in classDeclarationSyntax.BaseList.Types) {
            var typeSymbol = semanticModel.GetSymbolInfo(baseTypeSyntax.Type).Symbol;
            
            if (typeSymbol == null) continue;

            var typeFullyQualifiedName = typeSymbol.ToDisplayString();

            if (typeFullyQualifiedName == fullyQualifiedName) {
                return true;
            }
        }

        return false;
    }

    public static IEnumerable<PropertyDeclarationSyntax> GetEveryPropertyOfType(
        this SyntaxList<MemberDeclarationSyntax> members, 
        SemanticModel semanticModel, 
        string fullyQualifiedName) 
    {
        foreach (var structMemberDeclaration in members) {
            if (structMemberDeclaration is not PropertyDeclarationSyntax propertyDeclarationSyntax) continue;

            var typeSyntax = propertyDeclarationSyntax.Type;
            var typeSymbol = semanticModel.GetSymbolInfo(typeSyntax).Symbol;

            if (typeSymbol == null) continue;

            var typeName = typeSymbol.ToDisplayString();

            if (typeName == fullyQualifiedName) {
                yield return propertyDeclarationSyntax;
            }
        }
    }

    public static (string? namespaceName, string className) SeparateFromFullyQualifiedName(this string fullyQualifiedClassName) {
        int dotInd = fullyQualifiedClassName.LastIndexOf('.');

        if (dotInd != -1) {
            string namespaceName = fullyQualifiedClassName.Substring(0, dotInd);
            string className = fullyQualifiedClassName.Substring(dotInd + 1);

            return (namespaceName, className);
        }
        else {
            string? namespaceName = null;
            string className = fullyQualifiedClassName;
            
            return (namespaceName, className);
        }
    }

    public static string ExtractConcreteClassName(this string fullyQualifiedClassName) {
        int dotInd = fullyQualifiedClassName.LastIndexOf('.');

        return dotInd != -1 ? fullyQualifiedClassName.Substring(dotInd + 1) : fullyQualifiedClassName;
    }
}