using System.Linq;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generators.Extensions;

public static class ISymbolExtensions {
    public static bool GetAttribute(this ISymbol symbol, string attributeClassName, out AttributeData attributeData) {
        var attributesData = symbol.GetAttributes();

        if (attributesData.FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == attributeClassName) is { } ad) {
            attributeData = ad;
            return true;
        }

        attributeData = null!; // let user check for bool
        return false;
    }

    public static bool IsDerivedFrom(this ITypeSymbol type, string baseFullyQualifiedName) {
        ITypeSymbol? baseType = type.BaseType;

        while (baseType != null) {
            if (baseType.ToDisplayString() == baseFullyQualifiedName) return true;
            else baseType = baseType.BaseType;
        }

        return false;
    }

    public static string GetBackingFieldName(this IPropertySymbol propertySymbol) => $"<{propertySymbol.Name}>k__BackingField";

    public static IFieldSymbol? GetBackingField(this IPropertySymbol propertySymbol) {
        var fieldName = GetBackingFieldName(propertySymbol);
        var containingType = propertySymbol.ContainingType;

        var foundMembers = containingType.GetMembers(fieldName).OfType<IFieldSymbol>();

        return foundMembers.FirstOrDefault();
    }
}