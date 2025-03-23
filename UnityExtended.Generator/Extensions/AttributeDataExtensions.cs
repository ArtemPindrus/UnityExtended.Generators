using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public static class AttributeDataExtensions {
    public static bool GetParamValueAt<T>(this AttributeData attributeData, int index, out T? value) {
        var arguments = attributeData.ConstructorArguments;

        if (arguments.Length >= index + 1) {
            value = (T?)arguments[index].Value;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }
}