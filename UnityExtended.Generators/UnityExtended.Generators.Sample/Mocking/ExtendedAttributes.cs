using System;

namespace UnityExtended.Core.Generators.Attributes {
    public enum In {
        Self,
        Children,
        Parent,
    }

    public class MonoBehaviourAttribute : Attribute {
        
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SetVisualElementAt : Attribute {
        public SetVisualElementAt(int index) {}
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class GetComponentAttribute : MonoBehaviourAttribute {
        public GetComponentAttribute(In @in = In.Self, bool plural = false){}
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class GetComponentAheadAttribute : MonoBehaviourAttribute {
        public GetComponentAheadAttribute(In @in = In.Self, bool plural = false){}
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HandleInputAttribute : MonoBehaviourAttribute {
        public HandleInputAttribute(Type inputAssetType) {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SerializePropertyWithBackingAttribute : Attribute {
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectAttribute : MonoBehaviourAttribute {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class StartFoldoutGroupAttribute : Attribute {
        public StartFoldoutGroupAttribute(string groupName, int propertyOrder = -1){}
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class GeneratorHookAttribute(bool callBase = false) : Attribute {
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateCustomInspectorAttribute : Attribute {
        
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class EndFoldoutGroupAttribute : Attribute {
        
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DisplayAttribute : Attribute{}

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DisplayItemAttribute : Attribute {
        public DisplayItemAttribute(Type containerType){}
    }
}