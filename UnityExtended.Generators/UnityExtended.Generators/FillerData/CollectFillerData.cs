namespace UnityExtended.Generators.FillerData;

public readonly record struct CollectFillerData : IFillerData {
    public string FullyQualifiedGeneratedClassName { get; }
    
    public CollectFillerData(string fullyQualifiedGeneratedClassName) {
        FullyQualifiedGeneratedClassName = fullyQualifiedGeneratedClassName;
    }
}