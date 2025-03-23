using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityExtended.Core.Attributes;
using UnityExtended.Generators.Attributes;

#pragma warning disable

namespace MyNamespace {
    public partial class MyClass : MonoBehavior {
        [GetComponent]
        private Rigidbody rb;

        [GetComponent]
        private Image image;
        
        [GetComponent]
        private float s;
    }
}