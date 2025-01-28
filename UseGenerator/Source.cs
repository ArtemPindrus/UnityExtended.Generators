using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtended.Generator.Attributes;

namespace MyNamespace {
    public interface ISome{}
    
    [HandleInput(typeof(MyInput.InteractionActions))]
    public partial class Something : MonoBehavior {
        partial void MyInput_InteractionActions_OnattackPerformed(InputAction.CallbackContext callbackContext) {
            // do some
        }
    }
}