using System.Collections.Generic;
using System.Threading;
using Hierarchy;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public class ButtonAttributeData : IGenerateClass {
    private readonly CustomEditorClass customEditorClass;
    
    public Class GeneratedClass => customEditorClass;
    
    public ButtonAttributeData(INamedTypeSymbol classSymbol) {
        customEditorClass = CustomEditorClass.GetFor(classSymbol.ToDisplayString());

        customEditorClass.AddCreateGUIStatements("ButtonDrawer.DrawButtons(this);");
    }
    
    public static ButtonAttributeData Transform(GeneratorAttributeSyntaxContext context, CancellationToken _) {
        var classSymbol = context.TargetSymbol.ContainingType;

        return new ButtonAttributeData(classSymbol);
    }
}

public class ButtonAttributeClassEqualityComparer : IEqualityComparer<ButtonAttributeData> {
    public bool Equals(ButtonAttributeData x, ButtonAttributeData y) {
        return x.GeneratedClass.FullyQualifiedName == y.GeneratedClass.FullyQualifiedName;
    }

    public int GetHashCode(ButtonAttributeData obj) => obj.GeneratedClass.FullyQualifiedName.GetHashCode();
}