using System.Text.Json.Serialization.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityExtended.Generator;

using UnityExtended.Generator.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;


internal class Program {
    public static void Main(string[] args) {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
using UnityExtended.Generator.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityEngine {
    public class MonoBehavior {
        protected T GetComponent<T>() => default;
    }
}

namespace UnityEngine.InputSystem {
    public class InputAction {
        
    } 
    
    public partial class MyInput : IInputActionCollection2 {
        public struct InteractionActions {
            public InputAction attack => new();
            public InputAction interact => new();
        }
    }

    public partial class DragAndDropInput : IInputActionCollection2 {
        public struct DragAndDropActions {
            public InputAction Drag => new();
            public InputAction Drop => new();
        }
    }
    
    public interface IInputActionCollection2 {}
}

namespace MyNamespace {
    [HandleInput(typeof(MyInput.InteractionActions))]
    [HandleInput(typeof(DragAndDropInput.DragAndDropActions))]
    public partial class Something : MonoBehavior {
        [GetComponent] private MonoBehavior mono, some, field, another;

        [GetComponent] private object obj;

        partial void OnAttack() {
            
        }
    }
}
");
        var compilation = CSharpCompilation.Create("UseGenerator", new []{ syntaxTree });

        var generator = new UnityGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
    }
}

namespace UnityEngine {
    public class MonoBehavior {
        protected T GetComponent<T>() => default;
    }
}

namespace UnityEngine.InputSystem {
    public class InputAction {
        public event Action<CallbackContext> performed;
        public event Action<CallbackContext> canceled;
        public event Action<CallbackContext> started; 
        
        public struct CallbackContext{}
    } 
    
    public partial class MyInput : IInputActionCollection2 {
        public struct InteractionActions {
            public InputAction attack => new();
            public InputAction interact => new();
        }
    }

    public partial class DragAndDropInput : IInputActionCollection2 {
        public struct DragAndDropActions {
            public InputAction Drag => new();
            public InputAction Drop => new();
        }
    }
    
    public interface IInputActionCollection2 {}
}

namespace MyNamespace {
    [HandleInput(typeof(MyInput.InteractionActions))]
    [HandleInput(typeof(DragAndDropInput.DragAndDropActions))]
    public partial class Something : MonoBehavior {
        [GetComponent] private MonoBehavior mono, some, field, another;

        [GetComponent] private object obj;

        partial void OnDragAndDropActions_DragPerformed(InputAction.CallbackContext context) {
            // do some
        }
    }
}