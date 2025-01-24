using System;

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

        public HandleInputAttribute(Type inputActionsType, params string[] inputActionNames) {
            InputActionsType = inputActionsType;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SerializePropertyWithBackingAttribute : Attribute {
    }
}
";

    public const string AwakeMethodSignature = "private void Awake()";
    public const string Awake2MethodSignature = "partial void Awake2()";
    
    public const string OnEnableMethodSignature = "private void OnEnable()";
    public const string OnEnable2MethodSignature = "partial void OnEnable2()";
    
    public const string OnDisableMethodSignature = "private void OnDisable()";
    public const string OnDisable2MethodSignature = "partial void OnDisable2()";

    public const string OnValidateMethodSignature = "private void OnValidate()";
    public const string OnValidate2MethodSignature = "private void OnValidate2()";

    public static readonly string[] InputActionPostfixes = ["Performed", "Started", "Canceled"];
}