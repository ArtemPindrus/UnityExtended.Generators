using UnityEngine.UIElements;

namespace UnityEngine {
    public static class Application {
        public static bool isPlaying = false;
    }
    
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

    public class CustomEditorAttribute : Attribute {
        public CustomEditorAttribute(Type type) {}
    }
    
    public class CanEditMultipleObjectsAttribute : Attribute { }
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

namespace UnityEngine.UIElements {
      public interface IVisualElementScheduledItem
  {
    /// <summary>
    ///        <para>
    /// Returns the VisualElement this object is associated with.
    /// </para>
    ///      </summary>
    VisualElement element { get; }

    /// <summary>
    ///        <para>
    /// Will be true when this item is scheduled. Note that an item's callback will only be executed when it's VisualElement is attached to a panel.
    /// </para>
    ///      </summary>
    bool isActive { get; }

    /// <summary>
    ///        <para>
    /// If not already active, will schedule this item on its VisualElement's scheduler.
    /// </para>
    ///      </summary>
    void Resume();

    /// <summary>
    ///        <para>
    /// Removes this item from its VisualElement's scheduler.
    /// </para>
    ///      </summary>
    void Pause();

    /// <summary>
    ///        <para>
    /// Cancels any previously scheduled execution of this item and re-schedules the item.
    /// </para>
    ///      </summary>
    /// <param name="delayMs">Minimum time in milliseconds before this item will be executed.</param>
    void ExecuteLater(long delayMs);

    /// <summary>
    ///        <para>
    /// Adds a delay to the first invokation.
    /// </para>
    ///      </summary>
    /// <param name="delayMs">The minimum number of milliseconds after activation where this item's action will be executed.</param>
    /// <returns>
    ///   <para>This ScheduledItem.</para>
    /// </returns>
    IVisualElementScheduledItem StartingIn(long delayMs);

    /// <summary>
    ///        <para>
    /// Repeats this action after a specified time.
    /// </para>
    ///      </summary>
    /// <param name="intervalMs">Minimum amount of time in milliseconds between each action execution.</param>
    /// <returns>
    ///   <para>This ScheduledItem.</para>
    /// </returns>
    IVisualElementScheduledItem Every(long intervalMs);

    IVisualElementScheduledItem Until(Func<bool> stopCondition);

    /// <summary>
    ///        <para>
    /// After specified duration, the item will be automatically unscheduled.
    /// </para>
    ///      </summary>
    /// <param name="durationMs">The total duration in milliseconds where this item will be active.</param>
    /// <returns>
    ///   <para>This ScheduledItem.</para>
    /// </returns>
    IVisualElementScheduledItem ForDuration(long durationMs);
  }
    
    public interface IVisualElementScheduler
    {
        /// <summary>
        ///        <para>
        /// Schedule this action to be executed later.
        /// </para>
        ///      </summary>
        /// <param name="updateEvent">The action to be executed.</param>
        /// <returns>
        ///   <para>Reference to the scheduled action.</para>
        /// </returns>
        IVisualElementScheduledItem Execute(Action updateEvent);
    }
    
    public class VisualElement {
        public IVisualElementScheduler schedule;
        
        public void Add(VisualElement element) => throw new NotSupportedException();
        
        public void AddAllSerializedProperties(object o) => throw new NotSupportedException();
    }

    public class FloatField : VisualElement {
        public float value;

        public string label;

        public bool enabledSelf;
    }

    public class Foldout : VisualElement {
        
    }
}

namespace UnityEditor {
    public class Editor {
        public object serializedObject;
        
        public object target { get; set; }
        
        public virtual VisualElement CreateInspectorGUI() => throw new NotSupportedException();
    }
}

namespace UnityExtended.Core.Extensions {
    public static class VisualElementExtensions {
        public static void AddAllSerializedProperties(this VisualElement container, object serializedObject) {
            throw new NotSupportedException();
        }
    }
}

namespace UnityExtended.Core.Attributes {
    // TODO: create Generator for this
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute {
        
    }
}

namespace UnityExtended.Core.Drawers {
    public static class ButtonDrawer {
        public static VisualElement DrawButtons(object target) => throw new NotSupportedException();
    }
}

    }
}

namespace EditorAttributes {
    public struct Void {}
    
    [AttributeUsage(AttributeTargets.Field)]
    public class FoldoutGroupAttribute : Attribute {
        public FoldoutGroupAttribute(string groupName, params string[] fields){}
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PropertyOrderAttribute : Attribute {
        public PropertyOrderAttribute(int order) {}
    }
}

[DisplayItem(typeof(Foldout), "NeckX", "NeckY")]
public class TargetAngles {
    private float neckX;
    
    public float NeckX {
        get => neckX;
    }
    
    public float NeckY { get; }
}