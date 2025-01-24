using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtended.Generator.Attributes;

namespace MyNamespace {
    public interface ISome{}
    
    public partial class Something : MonoBehavior {
        [SerializePropertyWithBacking]
        private MonoBehavior SomeObj {
            get => someObj;
            set => someObj = value;
        }

        [GetComponent]
        private MonoBehavior another;
    }
}