using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtended.Core.Generators.Attributes;

namespace UnityExtended.Generators.Sample;

[GeneratorHook]
[HandleInput(typeof(MyInput))]
public partial class Test4 : MonoBehaviour {
    public float z;
}

[GeneratorHook]
[Collect]
public partial class Here : MonoBehaviour {
    [GetComponent] public float y;

    [GetComponent] public object obj;
}

[GeneratorHook(callBase: true)]
public partial class There : Here {
    [GetComponent] private object shit;
}