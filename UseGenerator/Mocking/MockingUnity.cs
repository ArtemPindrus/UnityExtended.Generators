namespace UnityEngine {
    public enum RuntimeInitializeLoadType {
        SubsystemRegistration
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute {
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) {}
    }
    
    public class MonoBehavior {
        protected T GetComponent<T>() => throw new NotSupportedException();
    }
    
    public class SerializeFieldAttribute : Attribute {}
}

namespace UnityEngine.InputSystem {
    public class InputAction {
        public event Action<CallbackContext> performed;
        public event Action<CallbackContext> canceled;
        public event Action<CallbackContext> started; 
        
        public struct CallbackContext{}
    } 
    
    public partial class MyInput : IInputActionCollection2 {
        public InteractionActions Interaction;
        
        public struct InteractionActions {
            public InputAction Attack => new();
            public InputAction Interact => new();
        }
    }

    public partial class DragAndDropInput : IInputActionCollection2 {
        public DragAndDropActions DragAndDrop;
        
        public struct DragAndDropActions {
            public InputAction Drag => new();
            public InputAction Drop => new();
        }
    }
    
    public interface IInputActionCollection2 {}
}

namespace UnityExtended.Generators.Attributes {
    [AttributeUsage(AttributeTargets.Field)]
    public class GetComponentAttribute : Attribute {

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
        public StartFoldoutGroupAttribute(string groupName){}
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class EndFoldoutGroupAttribute : Attribute {
        
    }
}

namespace EditorAttributes {
    public struct Void {}
    
    [AttributeUsage(AttributeTargets.Field)]
    public class FoldoutGroupAttribute : Attribute {
        public FoldoutGroupAttribute(string groupName, params string[] fields){}
    }
}