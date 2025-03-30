namespace UnityExtended.Generators.Hierarchy;

public static class HierarchyHelper {
    public static string MethodSignatureIntoCallStatement(string signature) {
        int lastSpaceInd = signature.LastIndexOf(' ');
        
        return $"{signature.Substring(lastSpaceInd + 1)};";
    }
}