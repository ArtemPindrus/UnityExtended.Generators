using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public record struct Class {
    private readonly Dictionary<string,Method> methods = new(); // hashed by Qualifier
    private readonly HashSet<string> fields = new();

    public IEnumerable<Method> Methods => methods.Values;
    public IEnumerable<string> Fields => fields;
    
    public string FullyQualifiedClassName { get; }
    public string? NamespaceName { get; }
    public string ClassName { get; }
    
    public Class(string fullyQualifiedClassName, string? namespaceName, string className) {
        FullyQualifiedClassName = fullyQualifiedClassName;
        NamespaceName = namespaceName;
        ClassName = className;
    }

    public Class(ITypeSymbol classSymbol) {
        FullyQualifiedClassName = classSymbol.ToDisplayString();
        (NamespaceName, ClassName) = FullyQualifiedClassName.SeparateFromFullyQualifiedName();
    }

    public bool TryAddMethod(Method method) {
        string qualifier = method.Qualifier;
        
        if (methods.ContainsKey(qualifier)) return false;
        
        methods.Add(qualifier, method);
        return true;
    }

    public Method GetOrAddMethod(Method method) {
        if (methods.TryGetValue(method.Qualifier, out var existingMethod)) return existingMethod;
        else {
            if (!TryAddMethod(method)) throw new Exception("What?");
            else return method;
        }
    }
    
    public void TryAddMethods(IEnumerable<Method> addedMethods) {
        foreach (var method in addedMethods) {
            TryAddMethod(method);
        }
    }

    public bool TryAddField(string fieldDeclaration) {
        return fields.Add(fieldDeclaration);
    }
}