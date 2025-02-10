using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtended.Generators.Attributes;

namespace MyNamespace {
    [Collect]
    [HandleInput(typeof(MyInput.InteractionActions), typeof(DragAndDropInput.DragAndDropActions))]
    public partial class Something : MonoBehavior {
        [GetComponent] private MonoBehavior mono;

        [SerializePropertyWithBacking]
        private MonoBehavior M {
            get => m;
            set => throw new NotSupportedException();
        }
    }
}