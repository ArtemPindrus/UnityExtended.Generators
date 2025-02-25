using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UnityExtended.Generator;

namespace Hierarchy;

public class Class {
    private readonly List<Method> methods = [];
    private readonly List<string> fields = [];
    private readonly HashSet<string> implementations = [];
    private readonly List<string> attributes = [];

    public readonly HashSet<string> Constraints = [];
    public readonly HashSet<string> Usings = [];
    
    public IEnumerable<Method> Methods => methods;
    public IEnumerable<string> Fields => fields;
    public IEnumerable<string> Implementations => implementations;
    public IEnumerable<string> Attributes => attributes;
    
    public string FullyQualifiedName { get; }
    
    public string? NamespaceName { get; }
    public string Name { get; }

    public Class(string fullyQualifiedName) {
        FullyQualifiedName = fullyQualifiedName;
        (NamespaceName, Name) = FullyQualifiedName.SeparateFromFullyQualifiedName();
    }

    public void AddImplementation(string implementation) {
        implementations.Add(implementation);
    }
    
    public void AddAttribute(string attribute) {
        attributes.Add(attribute);
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

    public void AddMethods(params Method[] addedMethods) {
        foreach (var method in addedMethods) {
            AddMethod(method);
        }
    }

    public void AddField(string field) {
        string[] separateFields = field.Split('\n');

        foreach (var actualField in separateFields) {
            fields.Add(actualField);
        }
    }

    public void AddField(params string[] addedFields) {
        foreach (var field in addedFields) {
            AddField(field);
        }
    }
    
    public void AddUsing(string @using) {
        string[] usings = @using.Split('\n');
        
        AddUsing(usings);
    }

    public void AddUsing(params string[] usings) {
        foreach (var u in usings) {
            Usings.Add(u);
        }
    }

    public void Merge(Class other) {
        foreach (var otherMethod in other.methods) {
            if (methods.FirstOrDefault(x => x.Signature == otherMethod.Signature) is { } existing) {
                existing.Merge(otherMethod);
            } else methods.Add(otherMethod);
        }

        foreach (var otherField in other.fields) {
            fields.Add(otherField);
        }
    }

    public void AppendTo(IndentedStringBuilder stringBuilder) {
        // write constraints
        foreach (var con in Constraints) {
            stringBuilder.AppendLine(con);
        }
        
        // write usings
        foreach (var usingStatement in Usings) {
            stringBuilder.AppendLine($"using {usingStatement};");
        }

        stringBuilder.AppendLine();
        
        // open Namespace
        if (NamespaceName is { } namespaceName)
            stringBuilder.AppendLine($"namespace {namespaceName} {{").IncrementIndent();

        // write attributes
        foreach (var attribute in attributes) {
            stringBuilder.AppendLine(attribute);
        }
        
        // open Class
        stringBuilder.Append($"partial class {Name} ");

        if (implementations.Count > 0) {
            stringBuilder.Append(": ");
            
            foreach (var implementation in implementations) {
                stringBuilder.Append(implementation);
            }
        }

        stringBuilder.AppendLine("{").IncrementIndent();

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
        
        string closing = "";

        for (int i = 0; i < Constraints.Count; i++) {
            closing += "#endif\n";
        }
        
        stringBuilder.AppendLine(closing);
        // close constraints
    }

    public override string ToString() {
        IndentedStringBuilder builder = new();
        AppendTo(builder);

        return builder.ToString();
    }
}