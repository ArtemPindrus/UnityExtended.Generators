using UnityExtended.Generator;

namespace UnityExtended.Generators.FillerData;

public readonly record struct CreateCustomInspectorFillerData : IFillerData {
    public string FullyQualifiedGeneratedClassName { get; }
    
    public string BaseClassName { get; }
    
    public CreateCustomInspectorFillerData(string baseClassName) {
        FullyQualifiedGeneratedClassName = $"{baseClassName}Inspector";
        BaseClassName = baseClassName;
    }
}