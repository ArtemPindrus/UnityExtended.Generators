using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnityExtended.Generators.Hierarchy;

public abstract class HierarchyMember {
    private List<Diagnostic> diagnostics = new();

    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

    public void AddDiagnostic(string id, string category, string message, DiagnosticSeverity severity, int warningLevel) {
        Diagnostic diagnostic = Diagnostic.Create(id, category, message, severity, severity, true, warningLevel);
        
        diagnostics.Add(diagnostic);
    }

    public abstract void AppendTo(IndentedStringBuilder sb);
}