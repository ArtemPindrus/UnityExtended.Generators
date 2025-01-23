namespace UnityExtended.Generator;

public static class GeneratorHelper {
    public const string Attributes = @"
using System;

namespace UnityExtended.Generator.Attributes {
    [AttributeUsage(AttributeTargets.Field)]
    public class GetComponentAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HandleInputAttribute : Attribute {
        public Type InputActionsType;

        public HandleInputAttribute(Type inputActionsType) {
            InputActionsType = inputActionsType;
        }
    }
}
";
}