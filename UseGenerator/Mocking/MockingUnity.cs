namespace UnityEngine {
    public class MonoBehavior {
        protected T GetComponent<T>() => default;
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
            public InputAction attack => new();
            public InputAction interact => new();
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
}