using Microsoft.CodeAnalysis;
using UnityExtended.Generators.Extensions;

namespace UnityExtended.Generators.FillerData;

public readonly record struct GetComponentFillerData : IFillerData {
    public string FullyQualifiedGeneratedClassName { get; }
    public string FieldName { get; }
    public string FullyQualifiedTypeName { get; }
    public In In { get; }
    public bool Plural { get; }

    public GetComponentFillerData(string fullyQualifiedGeneratedClassName, IFieldSymbol field, AttributeData attribute) {
        FullyQualifiedGeneratedClassName = fullyQualifiedGeneratedClassName;
        FieldName = field.Name;
        FullyQualifiedTypeName = field.Type.ToDisplayString();

        attribute.GetParamValueAt(0, out In inValue);
        attribute.GetParamValueAt(1, out bool plural);

        In = inValue;
        Plural = plural;
    }
    
    public GetComponentFillerData(string fullyQualifiedGeneratedClassName, string fieldName,
        string fullyQualifiedTypeName, In @in = In.Self, bool plural = false) {
        FullyQualifiedGeneratedClassName = fullyQualifiedGeneratedClassName;
        FieldName = fieldName;
        FullyQualifiedTypeName = fullyQualifiedTypeName;
        In = @in;
        Plural = plural;
    }
}