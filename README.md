# Terms
## Blocked methods
Source generated method with engine-specific name (Awake, Start, OnEnable, OnDisable, etc...).

For every blocked method generator should create a partial continuation method (for Awake > Awake2, for Start > Start2, etc...) that the blocked method calls at the very end.

## Continuation method
Source generated partial method without definition that will be called at the end of a blocked method.

Can be defined by user to continue blocked method.

## Unobtrusive generators
Generators that add methods with generator-specific naming.
Those methods can be manually invoked by user. Otherwise classes should be decorated with [GeneratorHook].

## Obtrusive generators
Generators that create [Blocked methods](#blocked-methods).

The only obtrusive generator is the [HookGenerator](https://github.com/ArtemPindrus/UnityExtended.Generators/blob/main/UnityExtended.Generators/UnityExtended.Generators/Generators/HookGenerator.cs).

# Design
Most generators are unobtrusive. Their functions get tightly integrated by HookGenerator that targets [GeneratorHook].

# Attributes
## [GetComponentAttribute](https://github.com/ArtemPindrus/UnityExtended.Core/blob/main/Generators/Attributes/GetComponentAttribute.cs)
Code:
```csharp
[GeneratorHook]
public partial class Test : MonoBehaviour {
    [GetComponent]
    private Rigidbody rb;

    [GetComponent] 
    private Image image;

    [GetComponent] 
    private SomeOtherShit someOtherShit;
}
```

Generation:
```csharp
partial class Test {
    private void GetComponent_Generated() {
        // Reservation Main
        rb = GetComponent<UnityEngine.Rigidbody>();
        image = GetComponent<UnityEngine.UI.Image>();
        someOtherShit = GetComponent<SomeOtherShit>();
    }
}
```

Hook:
```cs
partial class Test {
    protected void Awake() {
        // Reservation Main
        PreGetComponent();
        GetComponent_Generated();
        PostGetComponent();
        Awake2();
    }

    partial void PreGetComponent();

    partial void PostGetComponent();

    partial void Awake2();
}

```

## [GetComponentAheadAttribute](https://github.com/ArtemPindrus/UnityExtended.Core/blob/main/Generators/Attributes/GetComponentAheadAttribute.cs)
Code:
```csharp
namespace MyNamespace {
    public partial class MyClass : MonoBehavior {
        [SerializeField] // required here!
        [GetComponentAhead(In.Children, true)]
        private Rigidbody rb;

        [SerializeField]
        [GetComponentAhead(In.Parent)]
        private Image image;

        [SerializeField]
        [GetComponentAhead]
        private float s;
    }
}
```

Generation:
```csharp
namespace MyNamespace {
    partial class MyClass {
        private void OnValidate() {
            // Reservation GetComponentAheadRes
            PreGetComponentAhead();
            UnityEditor.EditorApplication.delayCall += ()=> {
                s = GetComponent<float>();
                image = GetComponentInParent<UnityEngine.Image>();
                rb = GetComponentsInChildren<UnityEngine.Rigidbody>();
            };
            PostGetComponentAhead();

            // Reservation FinishRes
            OnValidate2();
        }

        partial void OnValidate2();

        partial void PreGetComponentAhead();

        partial void PostGetComponentAhead();
    }
}
```

## [HandleInputAttribute](https://github.com/ArtemPindrus/UnityExtended.Core/blob/main/Generators/Attributes/HandleInputAttribute.cs)
Code:
```csharp
namespace UnityExtended.Generators.Sample;

[HandleInput(typeof(MyInput))]
public partial class Test : MonoBehavior {
    partial void OnAttackPerformed(InputAction.CallbackContext callbackContext) {
        // do shit!
    }

    partial void OnAttackStarted(InputAction.CallbackContext callbackContext) {
        // start shit!
    }

    partial void OnAttackCanceled(InputAction.CallbackContext callbackContext) {
        // cancel shit!
    }
}
```

Generation:
```csharp
namespace UnityExtended.Generators.Sample {
    partial class Test {
        private UnityEngine.InputSystem.MyInput MyInput;
        private UnityEngine.InputSystem.MyInput.InteractionActions InteractionActions;

        private void Awake() {
            MyInput = InputSingletonsManager.GetInstance<UnityEngine.InputSystem.MyInput>();
            InteractionActions = MyInput.Interaction;

            Awake2();
        }

        private void OnEnable() {
            InteractionActions.Attack.started += OnAttackStarted;
            InteractionActions.Attack.performed += OnAttackPerformed;
            InteractionActions.Attack.canceled += OnAttackCanceled;

            OnEnable2();
        }

        private void OnDisable() {
            InteractionActions.Attack.started -= OnAttackStarted;
            InteractionActions.Attack.performed -= OnAttackPerformed;
            InteractionActions.Attack.canceled -= OnAttackCanceled;

            OnDisable2();
        }

        partial void Awake2();

        partial void OnEnable2();

        partial void OnDisable2();

        partial void OnAttackStarted(InputAction.CallbackContext callbackContext);

        partial void OnAttackPerformed(InputAction.CallbackContext callbackContext);

        partial void OnAttackCanceled(InputAction.CallbackContext callbackContext);

        partial void OnInteractStarted(InputAction.CallbackContext callbackContext);

        partial void OnInteractPerformed(InputAction.CallbackContext callbackContext);

        partial void OnInteractCanceled(InputAction.CallbackContext callbackContext);
    }
}
```
