using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UnityExtended.Generator;

namespace Hierarchy;

public class Class : HierarchyMember {
    public const string FinishReservationID = "FinishRes";
    
    private static Dictionary<string, Class> FQNameToClass = new();
    
    public readonly HashSet<string> Constraints = [];
    public readonly HashSet<string> Usings = [];
    
    private readonly Dictionary<string,Method> methods = [];
    private readonly HashSet<string> fields = [];
    private readonly HashSet<string> implementations = [];
    private readonly List<string> attributes = [];
    
    /// <summary>
    /// Methods hashed by their signature.
    /// </summary>
    public ReadOnlyDictionary<string, Method> Methods { get; }
    public IReadOnlyCollection<string> Fields => fields;
    public IEnumerable<string> Implementations => implementations;
    public IEnumerable<string> Attributes => attributes;
    
    public string FullyQualifiedName { get; }
    public string? NamespaceName { get; }
    public string Name { get; }

    protected Class(string fullyQualifiedName) {
        FullyQualifiedName = fullyQualifiedName;
        (NamespaceName, Name) = FullyQualifiedName.SeparateFromFullyQualifiedName();
        
        Methods = new ReadOnlyDictionary<string, Method>(methods);
    }

    /// <summary>
    /// Gets cached Class or creates one and caches.
    /// </summary>
    /// <returns>true - got cached class. false - created new.</returns>
    public static bool GetOrCreate(string fullyQualifiedName, out Class c) {
        if (!GetCachedClass(fullyQualifiedName, out c)) {
            c = new Class(fullyQualifiedName);
            CacheClass(c);
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// Gets cached Class or creates one and caches.
    /// </summary>
    public static Class GetOrCreate(string fullyQualifiedName) {
        GetOrCreate(fullyQualifiedName, out var c);

        return c;
    }

    public static void ClearStaticState() {
        FQNameToClass.Clear();
    }

    protected static bool GetCachedClass(string fullyQualifiedName, out Class c) {
        if (FQNameToClass.TryGetValue(fullyQualifiedName, out c)) return true;

        return false;
    }

    protected static void CacheClass(Class c) {
        // TODO: cache instead of merging
        FQNameToClass.Add(c.FullyQualifiedName, c);
    }

    public virtual Class Finish() {
        if (methods.TryGetValue(GeneratorHelper.AwakeMethodSignature, out var awakeMethod)) {
            if (!awakeMethod.GetOrCreateReservation(FinishReservationID, out var awake)) {
                awake.AddStatement("Awake2();");
            } 
        }

        return this;
    }

    public void AddConstraint(string constraint) {
        Constraints.Add(constraint);
    }

    public void AddImplementation(string implementation) {
        implementations.Add(implementation);
    }
    
    public void AddAttribute(string attribute) {
        attributes.Add(attribute);
    }

    public bool GetOrCreateMethod(string signature, out Method m) {
        if (!methods.TryGetValue(signature, out m)) {
            m = new(signature);
            methods[signature] = m;
            
            return false;
        }

        return true;
    }

    public Method GetOrCreateMethod(string signature) {
        GetOrCreateMethod(signature, out var m);

        return m;
    }

    public Class AddFields(string fields) {
        string[] separateFields = fields.Split('\n');

        foreach (var field in separateFields) {
            this.fields.Add(field.Trim());
        }

        return this;
    }

    public Class AddFields(params string[] addedFields) {
        foreach (var field in addedFields) {
            AddFields(field.Trim());
        }
        
        return this;
    }
    
    public void AddUsings(string usings) {
        string[] usingsSeparated = usings.Split('\n');
        
        AddUsings(usingsSeparated);
    }

    public void AddUsings(params string[] usings) {
        foreach (var u in usings) {
            Usings.Add(u.Trim());
        }
    }

    public override void AppendTo(IndentedStringBuilder stringBuilder) {
        // write constraints
        foreach (var con in Constraints) {
            stringBuilder.AppendLine(con);
        }
        
        // write usings
        foreach (var usingStatement in Usings) {
            stringBuilder.AppendLine($"using {usingStatement};");
        }

        if (Usings.Any()) stringBuilder.AppendLine();
        
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

        if (Fields.Any()) stringBuilder.AppendLine();

        //// methods
        var lastMethod = methods.Values.Last();
        foreach (var method in methods.Values) {
            method.AppendTo(stringBuilder);

            if (method != lastMethod) stringBuilder.AppendLine();
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