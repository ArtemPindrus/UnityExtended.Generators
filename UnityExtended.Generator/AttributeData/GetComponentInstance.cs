using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public record struct GetComponentInstance(ITypeSymbol Type, string VariableName, In InParam, bool Plural);