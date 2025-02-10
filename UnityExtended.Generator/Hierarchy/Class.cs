using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UnityExtended.Generator;

namespace Hierarchy;

public class Class {
    private List<Method> methods = [];
    private List<string> fields = [];

    public IEnumerable<Method> Methods => methods;
    public IEnumerable<string> Fields => fields;
    
    public string FullyQualifiedName { get; }
    
    public string? NamespaceName { get; }
    public string Name { get; }

    public Class(string fullyQualifiedName) {
        FullyQualifiedName = fullyQualifiedName;
        (NamespaceName, Name) = FullyQualifiedName.SeparateFromFullyQualifiedName();
    }
    
    public void AddMethod(Method method) {
        Method? existing = methods.FirstOrDefault(x => x.Signature == method.Signature);

        if (existing != null) {
            existing.Merge(method);
        }
        else {
            methods.Add(method);
        }
    }

    public void AddMethods(params Method[] methods) {
        foreach (var method in methods) {
            AddMethod(method);
        }
    }

    public void AddField(string field) {
        fields.Add(field);
    }

    public void Merge(Class other) {
        foreach (var otherMethod in other.methods) {
            if (methods.FirstOrDefault(x => x.Signature == otherMethod.Signature) is { } existing) {
                existing.Merge(otherMethod);
            } else methods.Add(otherMethod);
        }

        foreach (var otherField in other.fields) {
            if (!fields.Contains(otherField)) fields.Add(otherField);
        }
    }

    public void AppendTo(IndentedStringBuilder stringBuilder) {
        // open Namespace
        if (NamespaceName is { } namespaceName)
            stringBuilder.AppendLine($"namespace {namespaceName} {{").IncrementIndent();

        // open Class
        stringBuilder.AppendLine($"partial class {Name} {{").IncrementIndent();

        //// fields
        foreach (var fieldDeclaration in Fields) {
            stringBuilder.AppendLine(fieldDeclaration);
        }

        stringBuilder.AppendLine();

        //// methods
        foreach (var method in methods) {
            foreach (var attribute in method.Attributes) {
                stringBuilder.AppendLine(attribute);
            }
            
            if (method.Signature.Contains("partial")) {
                stringBuilder.AppendLine($"{method.Signature};").AppendLine();
            }
            else {
                string methodSignature = method.Signature;
                stringBuilder.AppendLine($"{methodSignature} {{").IncrementIndent();
                
                foreach (var statement in method.Statements) {
                    stringBuilder.AppendLine(statement);
                }

                stringBuilder.DecrementIndent().AppendLine("}").AppendLine();
            }
        }

        stringBuilder.DecrementIndent().AppendLine("}"); 
        // close Class

        if (NamespaceName is not null)
            stringBuilder.DecrementIndent().AppendLine("}");
        // close Namespace
    }
}