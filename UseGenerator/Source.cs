using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityExtended.Generators.Attributes;

namespace MyNamespace {
    [Collect]
    [HandleInput(typeof(MyInput.InteractionActions), typeof(DragAndDropInput.DragAndDropActions))]
    public partial class Something : MonoBehavior {
        [field: Display]
        private TargetAngles TargetAngles { get; set; }

        [Display]
        private float x;

        [SerializePropertyWithBacking]
        private MonoBehavior M {
            get => new();
            set => throw new NotSupportedException();
        }
    }
}