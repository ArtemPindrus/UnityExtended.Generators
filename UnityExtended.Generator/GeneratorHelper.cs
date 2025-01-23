namespace UnityExtended.Generator;

public static class GeneratorHelper {
    public const string GetComponentAttribute = @"
namespace UnityExtended.Generator.Attributes {
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class GetComponentAttribute : System.Attribute {

    }
}
";

    public const string HandleInputAttribute = @"
namespace UnityExtended.Generator.Attributes {
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class HandleInputAttribute : System.Attribute {
        public System.Type InputActionsType;

        public HandleInputAttribute(System.Type inputActionsType) {
            InputActionsType = inputActionsType;
        }
    }
}
";
}