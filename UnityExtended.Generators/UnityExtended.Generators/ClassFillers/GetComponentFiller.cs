using UnityExtended.Generators.FillerData;
using UnityExtended.Generators.Helpers;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators.ClassFillers;

public class GetComponentFiller : IClassFiller<GetComponentFillerData> {
    private const string AwakeReservatioID = "GetComponentRes";
    
    public Class Fill(Class c, GetComponentFillerData data) {
        var awakeMethod = c.GetOrCreateMethod(GeneratorHelper.AwakeMethodSignature);
        c.GetOrCreateMethod(GeneratorHelper.Awake2MethodSignature);
        c.GetOrCreateMethod("partial void PreGetComponent()");
        c.GetOrCreateMethod("partial void PostGetComponent()");

        if (!awakeMethod.GetOrCreateReservation(AwakeReservatioID, out var awakeReservation)) {
            awakeReservation.AddStatements("""
                                      PreGetComponent();
                                      PostGetComponent();
                                      """);
        }

        string pluralPostfix = data.Plural ? "s" : "";
        string inPostfix = data.In.ToPostfix();
        awakeReservation.InsertStatement($"{data.FieldName} = GetComponent{pluralPostfix}{inPostfix}<{data.FullyQualifiedTypeName}>();", 1);

        return c;
    }
}