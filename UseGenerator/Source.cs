using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtended.Generators.Attributes;

namespace MyNamespace {
    [Collect]
    [HandleInput(typeof(MyInput.InteractionActions), typeof(DragAndDropInput.DragAndDropActions))]
    public partial class Something : MonoBehavior {
        private object o;
        
        [StartFoldoutGroup("MyStuff")]
        [GetComponent] private MonoBehavior mono;

        private MonoBehavior other;

        [EndFoldoutGroup] private MonoBehavior some;
        
        private object s;

        [SerializePropertyWithBacking]
        private MonoBehavior M {
            get => m;
            set => throw new NotSupportedException();
        }
    }
}