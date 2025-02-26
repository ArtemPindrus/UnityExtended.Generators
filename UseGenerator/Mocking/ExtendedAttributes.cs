using System;

namespace UnityExtended.Generators.Attributes {
    public enum In {
        Self,
        Children,
        Parent,
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class GetComponentAttribute : Attribute {
        public GetComponentAttribute(In @in = In.Self, bool plural = false){}
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class GetComponentAheadAttribute : Attribute {
        public GetComponentAheadAttribute(In @in = In.Self, bool plural = false){}
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HandleInputAttribute : Attribute {
        public HandleInputAttribute(params Type[] inputActionsType) {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SerializePropertyWithBackingAttribute : Attribute {
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class StartFoldoutGroupAttribute : Attribute {
        public StartFoldoutGroupAttribute(string groupName, int propertyOrder = -1){}
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class EndFoldoutGroupAttribute : Attribute {
        
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayAttribute : Attribute{}

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DisplayItemAttribute : Attribute {
        public DisplayItemAttribute(Type containerType, params string[] fieldsAndProperties){}
    }
}