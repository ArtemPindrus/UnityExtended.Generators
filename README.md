# Attributes
## [GetComponentAttribute](https://github.com/ArtemPindrus/UnityExtended.Core/blob/main/Generators/Attributes/GetComponentAttribute.cs)
Code:
```csharp
namespace MyNamespace {
    public partial class MyClass : MonoBehavior {
        [GetComponent(In.Children, true)]
        private Rigidbody rb;

        [GetComponent(In.Parent)]
        private Image image;
        
        [GetComponent]
        private float s;
    }
}
```

Generation:
```csharp
namespace MyNamespace {
    partial class MyClass {
        private void Awake() {
            // Reservation GetComponentRes
            PreGetComponent();
            s = GetComponent<float>();
            image = GetComponentInParent<UnityEngine.Image>();
            rb = GetComponentsInChildren<UnityEngine.Rigidbody>();
            PostGetComponent();

            // Reservation FinishRes
            Awake2();
        }

        partial void Awake2();

        partial void PreGetComponent();

        partial void PostGetComponent();
    }
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
