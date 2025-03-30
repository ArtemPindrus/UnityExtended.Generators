using System;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generators.FillerData.Helpers;

public static class DisplayFillerDataHelper {
    // TODO: not full spectrum 
    public static string ContainingTypeToVisualElementType(ITypeSymbol type) {
        return type.Name switch {
            nameof(String) => "TextField",
            nameof(Int32) => "IntegerField",
            nameof(UInt32) => "UnsignedIntegerField",
            nameof(Int64) => "LongField",
            nameof(UInt64) => "UnsignedLongField",
            nameof(Single) => "FloatField",
            nameof(Double) => "DoubleField",
            nameof(Boolean) => "Toggle"
        };
    }
}