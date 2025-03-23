using System.Linq;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator.Extensions;

public static class ISymbolExtensions {
    public static bool GetAttribute(this ISymbol symbol, string attributeClassName, out AttributeData attributeData) {
        var attributesData = symbol.GetAttributes();

        if (attributesData.FirstOrDefault(x => x.AttributeClass.ToDisplayString() == attributeClassName) is { } ad) {
            attributeData = ad;
            return true;
        }

        attributeData = null!; // let user check for bool
        return false;
    }
}