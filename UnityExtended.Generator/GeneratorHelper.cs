using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnityExtended.Generator;

public static class GeneratorHelper {
    public const string AttributesNamespace = "UnityExtended.Generators.Attributes";

    public const string AwakeMethodSignature = "private void Awake()";
    public const string Awake2MethodSignature = "partial void Awake2()";
    
    public const string OnEnableMethodSignature = "private void OnEnable()";
    public const string OnEnable2MethodSignature = "partial void OnEnable2()";
    
    public const string OnDisableMethodSignature = "private void OnDisable()";
    public const string OnDisable2MethodSignature = "partial void OnDisable2()";

    public const string OnValidateMethodSignature = "private void OnValidate()";
    public const string OnValidate2MethodSignature = "private void OnValidate2()";

    public static readonly string[] InputActionPostfixes = ["Performed", "Started", "Canceled"];
}