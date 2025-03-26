using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public class GetComponentAheadFiller : IClassFiller<GetComponentAheadFillerData> {
    private const string OnValidateReservationID = "GetComponentAheadRes";
    private const string PreSignatureName = "PreGetComponentAhead";
    private const string PostSignatureName = "PostGetComponentAhead";
    
    public Class Fill(Class c, GetComponentAheadFillerData data) {
        var onValidate = c.GetOrCreateMethod(GeneratorHelper.OnValidateMethodSignature);
        c.GetOrCreateMethod(GeneratorHelper.OnValidate2MethodSignature);
        c.GetOrCreateMethod($"partial void {PreSignatureName}()");
        c.GetOrCreateMethod($"partial void {PostSignatureName}()");

        if (!onValidate.GetOrCreateReservation(OnValidateReservationID, out var onValidateRes)) {
            onValidateRes.AddStatements($$"""
                                        {{PreSignatureName}}();
                                        UnityEditor.EditorApplication.delayCall += ()=> {
                                        };
                                        {{PostSignatureName}}();
                                        """);
        }
        
        string pluralPostfix = data.Plural ? "s" : "";
        string inPostfix = data.In.ToPostfix();
        onValidateRes.InsertStatement($"    {data.FieldName} = GetComponent{pluralPostfix}{inPostfix}<{data.FullyQualifiedTypeName}>();", 2);

        return c;
    }
}