using System;

namespace UnityExtended.Generator;

public static class StringExtensions {
    public static string ToLowerFirst(this string str) {
        Char[] chars = str.ToCharArray();
        chars[0] = char.ToLower(chars[0]);

        return new string(chars);
    }
}