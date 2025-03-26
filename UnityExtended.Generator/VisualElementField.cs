using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnityExtended.Generator;

public class VisualElementField {
    private List<VisualElementField> children = new();
    
    public readonly string FieldName;
    public readonly string TypeName;
    public readonly string? ObjectInitializer;
    public readonly int Order;
    
    private readonly bool isRoot;

    public IEnumerable<VisualElementField> Children => children;

    public VisualElementField(string fieldName, string typeName, int order = 0, string? objectInitializer = null) {
        FieldName = fieldName;
        TypeName = typeName;
        ObjectInitializer = objectInitializer;
        Order = order;
    }

    private VisualElementField() {
        FieldName = "root";
        TypeName = "VisualElement";
        isRoot = true;
    }

    public static VisualElementField CreateRoot() {
        return new VisualElementField();
    }

    public override string ToString() {
        var sb = new IndentedStringBuilder();
        
        AppendTo(sb);

        return sb.ToString();
    }

    private void AppendTo(IndentedStringBuilder sb) {
        SortChildren();

        if (isRoot) sb.AppendLine("var root = new VisualElement();");
        else sb.AppendLine($"{FieldName} = new {TypeName}() {{");
        
        if (ObjectInitializer != null) {
            sb.IncrementIndent();
            
            foreach (var line in ObjectInitializer.Split('\n')) {
                sb.AppendLine(line.Trim());
            }
            
            sb.DecrementIndent();
        }

        sb.AppendLine("};");

        sb.IncrementIndent();

        foreach (var child in children) {
            child.AppendTo(sb);
            
            sb.AppendLine($"{FieldName}.Add({child.FieldName});").AppendLine();
        }

        sb.DecrementIndent();
    }

    public void AddChild(VisualElementField child) {
        children.Add(child);
    }
    
    public void AddChildren(params VisualElementField[] child) {
        foreach (var visualElementField in child) children.Add(visualElementField);
    }

    public void SortChildren() {
        children = children.OrderBy(x => x.Order).ToList();
    }
}