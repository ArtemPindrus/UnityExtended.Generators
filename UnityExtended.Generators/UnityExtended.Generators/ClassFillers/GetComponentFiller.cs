using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public class GetComponentFiller : IClassFiller<GetComponentFillerData, Class> {
    public const string MethodName = $"GetComponent{GeneratorHelper.GenerationPostfix}";
    public const string MethodSignature = $"private void {MethodName}()";
    
    public Class Fill(Class c, GetComponentFillerData data) {
        var method = c.GetOrCreateMethod(MethodSignature);

        method.AddStatement($"{data.FieldName} = GetComponent<{data.FullyQualifiedTypeName}>();");
        
        return c;
    }
}