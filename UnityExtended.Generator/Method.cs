using System.Collections.Generic;

namespace UnityExtended.Generator;

public record struct Method {
    private readonly HashSet<string> statements = new();

    public IEnumerable<string> Statements => statements;
    
    /// <summary>
    /// Example: private void Awake
    /// </summary>
    public readonly string MethodSignature;
    public readonly string FullyQualifiedClassName;
    public readonly string Qualifier;

    public Method(string methodSignature, string fullyQualifiedClassName) {
        MethodSignature = methodSignature;
        FullyQualifiedClassName = fullyQualifiedClassName;

        Qualifier = fullyQualifiedClassName + methodSignature;
    }

    public bool TryAddStatement(StatementDeclaration statement) {
        if (statement.TargetMethod.Qualifier == Qualifier) {
            return statements.Add(statement.Statement);
        }

        return false;
    }
}