using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityExtended.Generators.Extensions;

internal static class GeneratorExtensions {
    public static bool Implements(this ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel, string fullyQualifiedName) {
        if (classDeclarationSyntax.BaseList == null || classDeclarationSyntax.BaseList.Types.Count == 0) return false;
        
        foreach (var baseTypeSyntax in classDeclarationSyntax.BaseList.Types) {
            var typeSymbol = ModelExtensions.GetSymbolInfo(semanticModel, baseTypeSyntax.Type).Symbol;
            
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
            var typeSymbol = ModelExtensions.GetSymbolInfo(semanticModel, typeSyntax).Symbol;

            if (typeSymbol == null) continue;

            var typeName = typeSymbol.ToDisplayString();

            if (typeName == fullyQualifiedName) {
                yield return propertyDeclarationSyntax;
            }
        }
    }
}