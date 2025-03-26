using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtended.Generators.Attributes;

namespace UnityExtended.Generators.Sample;

[HandleInput(typeof(MyInput))]
public partial class Test : MonoBehavior {
    [GetComponentAhead] private float t;
    [GetComponent] private MonoBehavior s;
    [GetComponent] private object o;
    [GetComponentAhead] private MonoBehavior te;

    partial void OnAttackPerformed(InputAction.CallbackContext callbackContext) {
        // do shit!
    }

    partial void OnAttackStarted(InputAction.CallbackContext callbackContext) {
        throw new System.NotImplementedException();
    }

    partial void OnAttackCanceled(InputAction.CallbackContext callbackContext) {
        throw new System.NotImplementedException();
    }
}