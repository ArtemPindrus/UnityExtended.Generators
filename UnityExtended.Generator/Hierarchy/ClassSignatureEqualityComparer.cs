using System.Collections.Generic;

namespace Hierarchy;

public class ClassSignatureEqualityComparer : IEqualityComparer<Class> {
    public static readonly ClassSignatureEqualityComparer Default = new();
    
    public bool Equals(Class x, Class y) => x.FullyQualifiedName == y.FullyQualifiedName;

    public int GetHashCode(Class obj) => obj.FullyQualifiedName.GetHashCode();
}