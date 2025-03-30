using UnityExtended.Generator;

namespace UnityExtended.Generators.FillerData;

public readonly record struct CreateCustomInspectorFillerData : IFillerData {
    public string FullyQualifiedGeneratedClassName { get; }
    
    public CreateCustomInspectorFillerData(string fullyQualifiedBaseClassName) {
        FullyQualifiedGeneratedClassName = fullyQualifiedBaseClassName;
    }
}