# Attributes
## [GetComponentAttribute](https://github.com/ArtemPindrus/UnityExtended.Core/blob/main/Generators/Attributes/GetComponentAttribute.cs)
Code:
```csharp
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
            image = GetComponent<UnityEngine.Image>();
            rb = GetComponent<UnityEngine.Rigidbody>();
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
