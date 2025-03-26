using System;

namespace UnityExtended.Generators.Extensions;

public static class StringExtensions {
    public static string NormalizeBackingFieldName(this string backingFieldName) =>
        backingFieldName
            .Replace("<", "")
            .Replace(">k__BackingField", "");
    
    public static string ToLowerFirst(this string str) {
        Char[] chars = str.ToCharArray();
        chars[0] = char.ToLower(chars[0]);

        return new string(chars);
    }
    
    public static string ToUpperFirst(this string str) {
        Char[] chars = str.ToCharArray();
        chars[0] = char.ToUpper(chars[0]);

        return new string(chars);
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