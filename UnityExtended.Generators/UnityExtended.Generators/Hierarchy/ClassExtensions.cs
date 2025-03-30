namespace UnityExtended.Generators.Hierarchy;

public static class ClassExtensions {
    public static void EnsureMethodIsCreatedAndCallIt(this Class c, string methodSignature, Method caller) {
        c.GetOrCreateMethod(methodSignature);
        
        caller.AddStatement(HierarchyHelper.MethodSignatureIntoCallStatement(methodSignature));
    }
}