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
            // Reservation Main

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
        [GetComponentAhead(In.Children, true)]
        private Rigidbody rb;

        [GetComponentAhead(In.Parent)]
        private Image image;
        
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
            // Reservation Main

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
