using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UnityExtended.Generators;

namespace UnityExtended.Generator;

public class VisualElementCodeDefinition {
    private EquatableList<VisualElementCodeDefinition> children = new();
    
    public readonly string VariableName;

    
    /// <summary>
    /// Concrete type of VisualElement.
    /// </summary>
    public readonly string TypeName;
    
    public VisualElementCodeDefinition? Parent { get; set; }
    
    /// <summary>
    /// Statements put into object initializer of this variable.
    /// </summary>
    public string? ObjectInitializer { get; init; }
    
    /// <summary>
    /// Gets a value indicating whether the variable a field.
    /// </summary>
    public bool IsField { get; init; }
    
    /// <summary>
    /// Order of VisualElement within it's parent's hierarchy.
    /// </summary>
    public int Order { get; init; }
    public IReadOnlyList<VisualElementCodeDefinition> Children => children;

    public VisualElementCodeDefinition(string variableName, string typeName = "VisualElement") {
        VariableName = variableName;
        TypeName = typeName;
    }

    /// <summary>
    /// Traverse every children recursively to find variables that are fields.
    /// </summary>
    /// <returns></returns>
    public VisualElementCodeDefinition[] GetHierarchyFields() {
        List<VisualElementCodeDefinition> fields = [];
        
        if (IsField) fields.Add(this);
        
        TraverseChildren(this);

        return fields.ToArray();

        void TraverseChildren(VisualElementCodeDefinition element) {
            foreach (var child in element.children) {
                if (child.IsField) fields.Add(child);
                if (child.Children.Count != 0) TraverseChildren(child);
            }
        }
    }

    public override string ToString() {
        var sb = new IndentedStringBuilder();
        
        AppendTo(sb);

        return sb.ToString();
    }

    private void AppendTo(IndentedStringBuilder sb) {
        SortChildren();

        string variableDeclaration = IsField ? VariableName : $"var {VariableName}";
        
        sb.Append($"{variableDeclaration} = new {TypeName}()");

        if (ObjectInitializer != null) {
            sb.AppendLine("{");
            sb.IncrementIndent();

            foreach (var line in ObjectInitializer.Split('\n')) {
                sb.AppendLine(line.Trim());
            }

            sb.DecrementIndent();

            sb.Append("}");
        }
        
        sb.AppendLine(";");

        sb.IncrementIndent();

        foreach (var child in children) {
            child.AppendTo(sb);
        }
        
        sb.DecrementIndent();
        
        if (Parent != null) sb.AppendLine($"{Parent.VariableName}.Add({VariableName});");
    }

    public void AddChild(VisualElementCodeDefinition child) {
        children.Add(child);
        child.Parent = this;
    }
    
    public VisualElementCodeDefinition AddChildren(params VisualElementCodeDefinition[] childen) {
        foreach (var child in childen) AddChild(child);

        return this;
    }

    public void SortChildren() {
        children = children.OrderBy(x => x.Order).ToEquatableList();
    }
}