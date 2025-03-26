using System;

namespace UnityExtended.Generators.FillerData;

public enum In {
    Self,
    Children,
    Parent
}

public static class InExtensions {
    public static string ToPostfix(this In inValue) => inValue switch {
        In.Self => "",
        In.Children => "InChildren",
        In.Parent => "InParent",
        _ => throw new NotSupportedException()
    };
}